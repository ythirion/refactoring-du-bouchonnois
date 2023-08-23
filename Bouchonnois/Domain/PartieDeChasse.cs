using System.Collections.Immutable;
using Bouchonnois.Domain.Apéro;
using Bouchonnois.Domain.Commands;
using Domain.Core;
using LanguageExt;
using static System.String;
using static Bouchonnois.Domain.Error;
using static Bouchonnois.Domain.PartieStatus;
using static LanguageExt.Unit;

namespace Bouchonnois.Domain
{
    public sealed class PartieDeChasse : Aggregate
    {
        private readonly Arr<Chasseur> _chasseurs = Arr<Chasseur>.Empty;

        // TODO : à supprimer à terme
        private readonly List<Event> _events = new();
        public IReadOnlyList<Chasseur> Chasseurs => _chasseurs.ToImmutableArray();
        public Terrain? Terrain { get; }
        public PartieStatus Status { get; private set; }
        public IReadOnlyList<Event> Events => _events.ToImmutableArray();

        private PartieDeChasse(Guid id, Func<DateTime> timeProvider) : base(timeProvider) => Id = id;

        #region Create

        private PartieDeChasse(Guid id,
            Func<DateTime> timeProvider,
            Terrain terrain,
            Chasseur[] chasseurs)
            : this(id, timeProvider)
        {
            Id = id;
            _chasseurs = chasseurs.ToArr();
            Terrain = terrain;
            Status = EnCours;
            _events = new List<Event>();

            EmitPartieDémarrée(timeProvider);
        }

        public static Either<Error, PartieDeChasse> Create(Func<DateTime> timeProvider,
            DemarrerPartieDeChasse demarrerPartieDeChasse)
        {
            if (!IsTerrainValide(demarrerPartieDeChasse.TerrainDeChasse))
            {
                return AnError("Impossible de démarrer une partie de chasse sur un terrain sans galinettes");
            }

            if (!ContainsChasseurs(demarrerPartieDeChasse.Chasseurs.ToArray()))
            {
                return AnError("Impossible de démarrer une partie de chasse sans chasseurs...");
            }

            if (AuMoinsUnChasseurNaPasDeBalles(demarrerPartieDeChasse.Chasseurs.ToArray()))
            {
                return AnError("Impossible de démarrer une partie de chasse avec un chasseur sans balle(s)...");
            }

            return new PartieDeChasse(
                Guid.NewGuid(),
                timeProvider,
                new Terrain(demarrerPartieDeChasse.TerrainDeChasse.Nom,
                    demarrerPartieDeChasse.TerrainDeChasse.NbGalinettes),
                demarrerPartieDeChasse.Chasseurs.Select(c => new Chasseur(c.Nom, c.NbBalles)).ToArray()
            );
        }

        private static bool IsTerrainValide(TerrainDeChasse terrainDeChasse) => terrainDeChasse.NbGalinettes > 0;
        private static bool ContainsChasseurs(Commands.Chasseur[] chasseurs) => chasseurs.Any();

        private static bool AuMoinsUnChasseurNaPasDeBalles(Commands.Chasseur[] chasseurs)
            => chasseurs.Exists(c => c.NbBalles == 0);

        private void EmitPartieDémarrée(Func<DateTime> timeProvider)
        {
            var chasseursToString = Join(", ", _chasseurs.Select(c => c.Nom + $" ({c.BallesRestantes} balles)"));
            EmitEvent($"La partie de chasse commence à {Terrain!.Nom} avec {chasseursToString}", timeProvider);
        }

        #endregion

        #region Apéro

        public Either<Error, Unit> PrendreLapéro(Func<DateTime> timeProvider)
        {
            if (DuringApéro())
            {
                return AnError("On est déjà en plein apéro");
            }

            if (DéjàTerminée())
            {
                return AnError("La partie de chasse est déjà terminée");
            }

            RaiseEvent(new ApéroDémarré(Id, timeProvider()));
            EmitEvent("Petit apéro", timeProvider);

            return Default;
        }

        private void Apply(ApéroDémarré @event) => Status = PartieStatus.Apéro;

        #endregion

        #region Reprendre

        public Either<Error, PartieDeChasse> Reprendre(Func<DateTime> timeProvider)
        {
            if (DéjàEnCours())
            {
                return AnError("La partie de chasse est déjà en cours");
            }

            if (DéjàTerminée())
            {
                return AnError("La partie de chasse est déjà terminée");
            }

            Status = EnCours;
            EmitEvent("Reprise de la chasse", timeProvider);

            return this;
        }

        #endregion

        #region Consulter

        public Either<Error, string> Consulter() =>
            Join(
                Environment.NewLine,
                Events
                    .OrderByDescending(@event => @event.Date)
                    .Select(@event => @event.ToString())
            );

        #endregion

        #region Terminer

        public Either<Error, string> Terminer(Func<DateTime> timeProvider)
        {
            if (DéjàTerminée())
            {
                return AnError("Quand c'est fini, c'est fini");
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
                EmitEvent(
                    $"La partie de chasse est terminée, vainqueur : {Join(", ", classement[0].Select(c => $"{c.Nom} - {c.NbGalinettes} galinettes"))}",
                    timeProvider);
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

        public Either<Error, PartieDeChasse> Tirer(
            string chasseur,
            Func<DateTime> timeProvider)
            => Tirer(chasseur,
                timeProvider,
                debutMessageSiPlusDeBalles: $"{chasseur} tire",
                _ => EmitEvent($"{chasseur} tire", timeProvider));

        private Either<Error, PartieDeChasse> Tirer(
            string chasseur,
            Func<DateTime> timeProvider,
            string debutMessageSiPlusDeBalles,
            Action<Chasseur>? continueWith = null)
        {
            if (DuringApéro())
            {
                return EmitAndReturn(
                    $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!",
                    timeProvider);
            }

            if (DéjàTerminée())
            {
                return EmitAndReturn($"{chasseur} veut tirer -> On tire pas quand la partie est terminée",
                    timeProvider);
            }

            if (!ChasseurExiste(chasseur))
            {
                return EmitAndReturn($"Chasseur inconnu {chasseur}", timeProvider);
            }

            var chasseurQuiTire = RetrieveChasseur(chasseur);

            if (!chasseurQuiTire.AEncoreDesBalles())
            {
                return EmitAndReturn($"{debutMessageSiPlusDeBalles} -> T'as plus de balles mon vieux, chasse à la main",
                    timeProvider);
            }

            chasseurQuiTire.ATiré();
            continueWith?.Invoke(chasseurQuiTire);

            return this;
        }

        private Either<Error, PartieDeChasse> EmitAndReturn(string message, Func<DateTime> timeProvider)
        {
            EmitEvent(message, timeProvider);
            return AnError(message);
        }

        #endregion

        #region Tirer sur une Galinette

        public Either<Error, PartieDeChasse> TirerSurUneGalinette(
            string chasseur,
            Func<DateTime> timeProvider)
        {
            if (Terrain!.NbGalinettes == 0)
            {
                return EmitAndReturn(
                    $"T'as trop picolé mon vieux, t'as rien touché",
                    timeProvider);
            }

            return Tirer(chasseur,
                timeProvider,
                debutMessageSiPlusDeBalles: $"{chasseur} veut tirer sur une galinette",
                c =>
                {
                    c.ATué();
                    Terrain.UneGalinetteEnMoins();
                    EmitEvent($"{chasseur} tire sur une galinette", timeProvider);
                });
        }

        #endregion

        private bool DuringApéro() => Status == PartieStatus.Apéro;
        private bool DéjàTerminée() => Status == Terminée;
        private bool DéjàEnCours() => Status == EnCours;
        private bool ChasseurExiste(string chasseur) => _chasseurs.Exists(c => c.Nom == chasseur);
        private Chasseur RetrieveChasseur(string chasseur) => _chasseurs.ToList().Find(c => c.Nom == chasseur)!;

        private void EmitEvent(string message, Func<DateTime> timeProvider) =>
            _events.Add(new Event(timeProvider(), message));
    }
}