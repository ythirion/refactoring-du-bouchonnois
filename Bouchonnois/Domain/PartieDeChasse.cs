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
            EmitEvent($"La partie de chasse commence à {Terrain.Nom} avec {chasseursToString}", timeProvider);
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
            EmitEvent("Petit apéro", timeProvider);
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
            EmitEvent("Reprise de la chasse", timeProvider);
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
                EmitEvent("La partie de chasse est terminée, vainqueur : Brocouille", timeProvider);
            }
            else
            {
                result = Join(", ", classement[0].Select(c => c.Nom));
                EmitEvent($"La partie de chasse est terminée, vainqueur : {Join(", ", classement[0].Select(c => $"{c.Nom} - {c.NbGalinettes} galinettes"))}", timeProvider);
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

            EmitEvent($"{chasseur} tire", timeProvider);
        }

        #endregion

        #region Tirer sur une Galinette

        public void TirerSurUneGalinette(
            string chasseur,
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
                    EmitEvent($"{chasseur} tire sur une galinette", timeProvider);
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
                EmitEventAndSave($"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!", timeProvider, repository);
                throw new OnTirePasPendantLapéroCestSacré();
            }

            if (DéjàTerminée())
            {
                EmitEventAndSave($"{chasseur} veut tirer -> On tire pas quand la partie est terminée", timeProvider, repository);
                throw new OnTirePasQuandLaPartieEstTerminée();
            }

            if (!ChasseurExiste(chasseur))
            {
                throw new ChasseurInconnu(chasseur);
            }

            var chasseurQuiTire = RetrieveChasseur(chasseur);

            if (!chasseurQuiTire.AEncoreDesBalles())
            {
                EmitEventAndSave($"{debutMessageSiPlusDeBalles} -> T'as plus de balles mon vieux, chasse à la main", timeProvider, repository);
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

        private void EmitEvent(string message, Func<DateTime> timeProvider) =>
            _events.Add(new Event(timeProvider(), message));

        private void EmitEventAndSave(string message, Func<DateTime> timeProvider, IPartieDeChasseRepository repository)
        {
            EmitEvent(message, timeProvider);
            repository.Save(this);
        }
    }
}