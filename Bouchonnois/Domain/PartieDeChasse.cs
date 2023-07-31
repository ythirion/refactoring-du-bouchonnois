using System.Collections.Immutable;
using Bouchonnois.Domain.Exceptions;
using static System.String;
using static Bouchonnois.Domain.PartieStatus;

namespace Bouchonnois.Domain
{
    public sealed class PartieDeChasse
    {
        private readonly List<Chasseur> _chasseurs;
        private readonly List<Event> _events;

        public Guid Id { get; }
        public IReadOnlyList<Chasseur> Chasseurs => _chasseurs.ToImmutableArray();
        public Terrain Terrain { get; }
        public PartieStatus Status { get; private set; }
        public IReadOnlyList<Event> Events => _events.ToImmutableArray();

        #region Create

        private PartieDeChasse(Guid id,
            Func<DateTime> timeProvider,
            Terrain terrain,
            List<(string nom, int nbBalles)> chasseurs)
        {
            Id = id;
            _chasseurs = new List<Chasseur>();
            Terrain = terrain;
            Status = EnCours;
            _events = new List<Event>();

            chasseurs.ForEach(c => AddChasseur(c));
            EmitPartieDémarrée(timeProvider);
        }

        private void AddChasseur((string nom, int nbBalles) chasseur)
            => _chasseurs.Add(new Chasseur(chasseur.nom, chasseur.nbBalles));

        public static PartieDeChasse Create(
            Func<DateTime> timeProvider,
            (string nom, int nbGalinettes) terrainDeChasse,
            List<(string nom, int nbBalles)> chasseurs)
        {
            CheckTerrainValide(terrainDeChasse);
            CheckChasseursValides(chasseurs);

            return new PartieDeChasse(
                Guid.NewGuid(),
                timeProvider,
                new Terrain(terrainDeChasse.nom, terrainDeChasse.nbGalinettes),
                chasseurs
            );
        }

        private static void CheckTerrainValide((string nom, int nbGalinettes) terrainDeChasse)
        {
            if (terrainDeChasse.nbGalinettes <= 0)
            {
                throw new ImpossibleDeDémarrerUnePartieSansGalinettes();
            }
        }

        private static void CheckChasseursValides(List<(string nom, int nbBalles)> chasseurs)
        {
            if (chasseurs.Count == 0)
            {
                throw new ImpossibleDeDémarrerUnePartieSansChasseur();
            }

            if (AuMoinsUnChasseurNaPasDeBalles(chasseurs))
            {
                throw new ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle();
            }
        }

        private static bool AuMoinsUnChasseurNaPasDeBalles(IEnumerable<(string nom, int nbBalles)> chasseurs)
            => chasseurs.Any(c => c.nbBalles == 0);

        private void EmitPartieDémarrée(Func<DateTime> timeProvider)
        {
            var chasseursToString = Join(", ", _chasseurs.Select(c => c.Nom + $" ({c.BallesRestantes} balles)"));
            EmitEvent(timeProvider, $"La partie de chasse commence à {Terrain.Nom} avec {chasseursToString}");
        }

        #endregion


        #region Apéro

        public void PrendreLapéro(Func<DateTime> timeProvider)
        {
            if (DuringApéro())
            {
                throw new OnEstDéjàEnTrainDePrendreLapéro();
            }

            if (DéjàTerminée())
            {
                throw new OnPrendPasLapéroQuandLaPartieEstTerminée();
            }

            Status = Apéro;
            EmitEvent(timeProvider, "Petit apéro");
        }

        #endregion

        #region Reprendre

        public void Reprendre(Func<DateTime> timeProvider)
        {
            if (DéjàEnCours())
            {
                throw new LaChasseEstDéjàEnCours();
            }

            if (DéjàTerminée())
            {
                throw new QuandCestFiniCestFini();
            }

            Status = EnCours;
            EmitEvent(timeProvider, "Reprise de la chasse");
        }

        #endregion

        #region Consulter

        public string Consulter() =>
            Join(
                Environment.NewLine,
                Events
                    .OrderByDescending(@event => @event.Date)
                    .Select(@event => @event.ToString())
            );

        #endregion

        #region Terminer

        public string Terminer(Func<DateTime> timeProvider)
        {
            if (DéjàTerminée())
            {
                throw new QuandCestFiniCestFini();
            }

            Status = Terminée;
            string result;

            var classement = Classement();

            if (TousBrocouilles(classement))
            {
                result = "Brocouille";
                EmitEvent(timeProvider, "La partie de chasse est terminée, vainqueur : Brocouille");
            }
            else
            {
                result = Join(", ", classement[0].Select(c => c.Nom));
                EmitEvent(timeProvider,
                    $"La partie de chasse est terminée, vainqueur : {Join(", ", classement[0].Select(c => $"{c.Nom} - {c.NbGalinettes} galinettes"))}");
            }

            return result;
        }

        private List<IGrouping<int, Chasseur>> Classement()
            => Chasseurs
                .GroupBy(c => c.NbGalinettes)
                .OrderByDescending(g => g.Key)
                .ToList();

        private static bool TousBrocouilles(IEnumerable<IGrouping<int, Chasseur>> classement) =>
            classement.All(group => group.Key == 0);

        #endregion

        #region Tirer

        public void Tirer(
            string chasseur,
            Func<DateTime> timeProvider,
            IPartieDeChasseRepository repository)
        {
            Tirer(chasseur,
                timeProvider,
                repository,
                debutMessageSiPlusDeBalles: $"{chasseur} tire");

            EmitEvent(timeProvider, $"{chasseur} tire");
        }

        #endregion

        #region Tirer sur une Galinette

        public void TirerSurUneGalinette(string chasseur,
            Func<DateTime> timeProvider,
            IPartieDeChasseRepository repository)
        {
            if (Terrain.NbGalinettes == 0)
            {
                throw new TasTropPicoléMonVieuxTasRienTouché();
            }

            Tirer(chasseur,
                timeProvider,
                repository,
                debutMessageSiPlusDeBalles: $"{chasseur} veut tirer sur une galinette",
                c =>
                {
                    c.ATué();
                    Terrain.UneGalinetteEnMoins();
                    EmitEvent(timeProvider, $"{chasseur} tire sur une galinette");
                });
        }

        #endregion

        private void Tirer(
            string chasseur,
            Func<DateTime> timeProvider,
            IPartieDeChasseRepository repository,
            string debutMessageSiPlusDeBalles,
            Action<Chasseur>? continueWith = null)
        {
            if (DuringApéro())
            {
                EmitEventAndSave(timeProvider, repository,
                    $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");
                throw new OnTirePasPendantLapéroCestSacré();
            }

            if (DéjàTerminée())
            {
                EmitEventAndSave(timeProvider, repository,
                    $"{chasseur} veut tirer -> On tire pas quand la partie est terminée");
                throw new OnTirePasQuandLaPartieEstTerminée();
            }

            if (!ChasseurExiste(chasseur))
            {
                throw new ChasseurInconnu(chasseur);
            }

            var chasseurQuiTire = RetrieveChasseur(chasseur);

            if (!chasseurQuiTire.AEncoreDesBalles())
            {
                EmitEventAndSave(timeProvider, repository,
                    $"{debutMessageSiPlusDeBalles} -> T'as plus de balles mon vieux, chasse à la main");
                throw new TasPlusDeBallesMonVieuxChasseALaMain();
            }

            chasseurQuiTire.ATiré();
            continueWith?.Invoke(chasseurQuiTire);
        }

        private bool DuringApéro() => Status == Apéro;
        private bool DéjàTerminée() => Status == Terminée;
        private bool DéjàEnCours() => Status == EnCours;
        private bool ChasseurExiste(string chasseur) => _chasseurs.Exists(c => c.Nom == chasseur);
        private Chasseur RetrieveChasseur(string chasseur) => _chasseurs.Find(c => c.Nom == chasseur)!;

        private void EmitEvent(Func<DateTime> timeProvider, string message) =>
            _events.Add(new Event(timeProvider(), message));

        private void EmitEventAndSave(Func<DateTime> timeProvider, IPartieDeChasseRepository repository, string message)
        {
            EmitEvent(timeProvider, message);
            repository.Save(this);
        }
    }
}