using ArchUnitNET.Domain.Extensions;
using Bouchonnois.Domain;
using static Bouchonnois.Domain.PartieStatus;

namespace Bouchonnois.Tests.Builders
{
    public class PartieDeChasseBuilder
    {
        private int _nbGalinettes;
        private ChasseurBuilder[] _chasseurs = {Dédé(), Bernard(), Robert()};
        private List<PartieStatus> _status = new();
        private Event[] _events = Array.Empty<Event>();

        private PartieDeChasseBuilder(int nbGalinettes) => _nbGalinettes = nbGalinettes;

        public static PartieDeChasseBuilder SurUnTerrainRicheEnGalinettes(int nbGalinettes = 3) => new(nbGalinettes);

        public static Guid UnePartieDeChasseInexistante() => Guid.NewGuid();

        public PartieDeChasseBuilder Avec(params ChasseurBuilder[] chasseurs)
        {
            _chasseurs = chasseurs;
            return this;
        }

        public PartieDeChasseBuilder ALapéro()
        {
            _status.Add(Apéro);
            return this;
        }

        public PartieDeChasseBuilder Terminée()
        {
            _status.Add(PartieStatus.Terminée);
            return this;
        }

        public PartieDeChasseBuilder Events(params Event[] events)
        {
            _events = events;
            return this;
        }

        public PartieDeChasse Build(Func<DateTime> timeProvider, IPartieDeChasseRepository repository)
        {
            var builtChasseurs = _chasseurs.Select(c => c.Build());
            var chasseursSansBalles = builtChasseurs.Where(c => c.BallesRestantes == 0).Select(c => c.Nom);

            var partieDeChasse = PartieDeChasse.Create(
                timeProvider,
                ("Pitibon sur Sauldre", _nbGalinettes),
                builtChasseurs
                    .Select(c => (c.Nom, c.BallesRestantes > 0 ? c.BallesRestantes : 1))
                    .ToList()
            );

            TirerSurLesGalinettes(partieDeChasse, timeProvider, repository, builtChasseurs);
            TirerDansLeVide(partieDeChasse, timeProvider, repository, chasseursSansBalles);

            ChangeStatus(partieDeChasse, timeProvider);

            partieDeChasse.Events = _events.ToList();

            return partieDeChasse;
        }

        private static void TirerDansLeVide(
            PartieDeChasse partieDeChasse,
            Func<DateTime> timeProvider,
            IPartieDeChasseRepository repository,
            IEnumerable<string> chasseursSansBalles) =>
            chasseursSansBalles.ForEach(c => partieDeChasse.Tirer(c, timeProvider, repository));

        private static void TirerSurLesGalinettes(
            PartieDeChasse partieDeChasse,
            Func<DateTime> timeProvider,
            IPartieDeChasseRepository repository,
            IEnumerable<Chasseur> builtChasseurs) =>
            partieDeChasse
                .Chasseurs
                .ForEach(c =>
                {
                    var built = builtChasseurs.First(x => x.Nom == c.Nom);
                    Repeat(built.NbGalinettes,
                        () => partieDeChasse.TirerSurUneGalinette(built.Nom, timeProvider, repository));
                });

        private void ChangeStatus(PartieDeChasse partieDeChasse, Func<DateTime> timeProvider) =>
            _status.ForEach(status => ChangeStatus(partieDeChasse, status, timeProvider));

        private void ChangeStatus(PartieDeChasse partieDeChasse, PartieStatus status, Func<DateTime> timeProvider)
        {
            if (status == PartieStatus.Terminée) partieDeChasse.Terminer(timeProvider);
            else if (status == Apéro) partieDeChasse.PrendreLapéro(timeProvider);
        }

        private static void Repeat(int times, Action call)
        {
            while (times > 0)
            {
                call();
                times--;
            }
        }
    }
}