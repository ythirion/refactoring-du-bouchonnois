using Bouchonnois.Domain.Apéro;
using Bouchonnois.Domain.Démarrer;
using Bouchonnois.Domain.Reprendre;
using Bouchonnois.Domain.Terminer;
using Bouchonnois.Domain.Tirer;
using Domain.Core;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using static System.String;
using static Bouchonnois.Domain.Error;
using static Bouchonnois.Domain.PartieStatus;
using static Domain.Core.AsyncHelper;
using static LanguageExt.Unit;

namespace Bouchonnois.Domain
{
    public sealed class PartieDeChasse : Aggregate
    {
        private PartieStatus _status;
        private Arr<Chasseur> _chasseurs = Arr<Chasseur>.Empty;
        private Terrain? _terrain;

        private PartieDeChasse(Guid id, Func<DateTime> timeProvider) : base(timeProvider) => Id = id;

        #region Create

        private PartieDeChasse(Guid id,
            Func<DateTime> timeProvider,
            Terrain terrain,
            Chasseur[] chasseurs) : this(id, timeProvider)
        {
            RaiseEvent((_, time) =>
                new PartieDeChasseDémarrée(id,
                    time,
                    new TerrainCréé(terrain.Nom, terrain.NbGalinettes),
                    chasseurs.Map(c => new ChasseurCréé(c.Nom, c.BallesRestantes)).ToArray()
                )
            );
        }

        public static Either<Error, PartieDeChasse> Create(
            Func<DateTime> timeProvider,
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
                new Terrain(
                    demarrerPartieDeChasse.TerrainDeChasse.Nom,
                    demarrerPartieDeChasse.TerrainDeChasse.NbGalinettes
                ),
                demarrerPartieDeChasse.Chasseurs.Select(c => new Chasseur(c.Nom, c.NbBalles)).ToArray()
            );
        }

        private void Apply(PartieDeChasseDémarrée @event)
        {
            Id = @event.Id;
            _chasseurs = @event.Chasseurs.Map(c => new Chasseur(c.Nom, c.BallesRestantes)).ToArray();
            _terrain = new Terrain(@event.Terrain.Nom, @event.Terrain.NbGalinettes);
            _status = EnCours;
        }

        private static bool IsTerrainValide(TerrainDeChasse terrainDeChasse) => terrainDeChasse.NbGalinettes > 0;
        private static bool ContainsChasseurs(Démarrer.Chasseur[] chasseurs) => chasseurs.Any();

        private static bool AuMoinsUnChasseurNaPasDeBalles(Démarrer.Chasseur[] chasseurs)
            => chasseurs.Exists(c => c.NbBalles == 0);

        #endregion

        #region Apéro

        public Either<Error, Unit> PrendreLapéro()
        {
            if (DuringApéro())
            {
                return AnError("On est déjà en plein apéro");
            }

            if (DéjàTerminée())
            {
                return AnError("La partie de chasse est déjà terminée");
            }

            RaiseEvent((id, time) => new ApéroDémarré(id, time));

            return Default;
        }

        private void Apply(ApéroDémarré @event) => _status = PartieStatus.Apéro;

        #endregion

        #region Reprendre

        public Either<Error, Unit> Reprendre()
        {
            if (DéjàEnCours())
            {
                return AnError("La partie de chasse est déjà en cours");
            }

            if (DéjàTerminée())
            {
                return AnError("La partie de chasse est déjà terminée");
            }

            RaiseEvent((id, time) => new PartieReprise(id, time));

            return Default;
        }

        private void Apply(PartieReprise @event) => _status = EnCours;

        #endregion

        #region Consulter

        public Either<Error, string> Consulter(IPartieDeChasseRepository repository)
            => RunSync(() => repository.EventsFor(Id)
                .Map(FormatEvents)
                .ValueUnsafe()
            );

        private static string FormatEvents(Seq<IEvent> events)
            => Join(Environment.NewLine,
                events.Map(@event => $"{@event.Date:HH:mm} - {@event}")
            );

        #endregion

        #region Terminer

        public Either<Error, string> Terminer()
        {
            if (DéjàTerminée())
            {
                return AnError("Quand c'est fini, c'est fini");
            }

            var classement = Classement();
            var (winners, nbGalinettes) = TousBrocouilles(classement)
                ? (new List<string> {"Brocouille"}, 0)
                : (classement[0].Map(c => c.Nom), classement[0].First().NbGalinettes);

            RaiseEvent((id, time) => new PartieTerminée(id, time, winners.ToSeq(), nbGalinettes));

            return Join(", ", winners);
        }

        private List<IGrouping<int, Chasseur>> Classement()
            => _chasseurs
                .GroupBy(c => c.NbGalinettes)
                .OrderByDescending(g => g.Key)
                .ToList();

        private static bool TousBrocouilles(IEnumerable<IGrouping<int, Chasseur>> classement) =>
            classement.All(group => group.Key == 0);

        private void Apply(PartieTerminée @event) => _status = Terminée;

        #endregion

        #region Tirer

        public Either<Error, Unit> Tirer(
            string chasseur)
            => Tirer(chasseur,
                intention: "tire",
                _ => RaiseEvent((id, time) => new ChasseurATiré(id, time, chasseur)));

        private Either<Error, Unit> Tirer(
            string chasseur,
            string intention,
            Action<Chasseur>? continueWith = null)
        {
            if (DuringApéro())
            {
                return RaiseEventAndReturnAnError((id, time) =>
                    new ChasseurAVouluTiréPendantLApéro(id, time, chasseur));
            }

            if (DéjàTerminée())
            {
                return RaiseEventAndReturnAnError((id, time) =>
                    new ChasseurAVouluTiréQuandPartieTerminée(id, time, chasseur));
            }

            if (!ChasseurExiste(chasseur))
            {
                return RaiseEventAndReturnAnError((id, time) => new ChasseurInconnuAVouluTiré(id, time, chasseur));
            }

            var chasseurQuiTire = RetrieveChasseur(chasseur);

            if (!chasseurQuiTire.AEncoreDesBalles())
            {
                return RaiseEventAndReturnAnError((id, time) =>
                    new ChasseurSansBallesAVouluTiré(id, time, chasseur, intention));
            }

            continueWith?.Invoke(chasseurQuiTire);

            return Default;
        }

        private void Apply(ChasseurATiré @event) => RetrieveChasseur(@event.Chasseur).ATiré();

        #endregion

        #region Tirer sur une Galinette

        public Either<Error, Unit> TirerSurUneGalinette(string chasseur)
            => _terrain is {NbGalinettes: 0}
                ? RaiseEventAndReturnAnError((id, time) => new ChasseurACruTiréSurGalinette(id, time, chasseur))
                : Tirer(chasseur,
                    intention: "veut tirer sur une galinette",
                    c => RaiseEvent((id, time) => new ChasseurATiréSurUneGalinette(id, time, chasseur)));

        private void Apply(ChasseurATiréSurUneGalinette @event)
        {
            var chasseur = RetrieveChasseur(@event.Chasseur);
            chasseur.ATiré();
            chasseur.ATué();
            _terrain!.UneGalinetteEnMoins();
        }

        #endregion

        private bool DuringApéro() => _status == PartieStatus.Apéro;
        private bool DéjàTerminée() => _status == Terminée;
        private bool DéjàEnCours() => _status == EnCours;
        private bool ChasseurExiste(string chasseur) => _chasseurs.Exists(c => c.Nom == chasseur);
        private Chasseur RetrieveChasseur(string chasseur) => _chasseurs.ToList().Find(c => c.Nom == chasseur)!;

        private IEvent RaiseEvent(Func<Guid, DateTime, IEvent> eventFactory)
        {
            var @event = eventFactory(Id, Time());
            RaiseEvent(@event);

            return @event;
        }

        private Error RaiseEventAndReturnAnError(Func<Guid, DateTime, IEvent> eventFactory) =>
            AnError(RaiseEvent(eventFactory).ToString()!);
    }
}