using ArchUnitNET.Domain.Extensions;
using Bouchonnois.Domain;
using Bouchonnois.Domain.Commands;
using static Bouchonnois.Domain.PartieStatus;
using static Bouchonnois.Tests.Builders.Functions;
using Chasseur = Bouchonnois.Domain.Chasseur;

namespace Bouchonnois.Tests.Builders
{
    public class PartieDeChasseBuilder
    {
        private readonly int _nbGalinettes;
        private ChasseurBuilder[] _chasseurs = {Dédé(), Bernard(), Robert()};
        private readonly List<PartieStatus> _status = new();

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

        public PartieDeChasse Build(Func<DateTime> timeProvider, IPartieDeChasseRepository repository)
        {
            var builtChasseurs = _chasseurs.Select(c => c.Build());
            var chasseursSansBalles = builtChasseurs.Where(c => c.BallesRestantes == 0).Select(c => c.Nom);

            var partieDeChasse = PartieDeChasse.Create(
                timeProvider,
                new DemarrerPartieDeChasse(
                    new TerrainDeChasse("Pitibon sur Sauldre", _nbGalinettes),
                    builtChasseurs
                        .Select(c => new Domain.Commands.Chasseur(c.Nom, c.BallesRestantes > 0 ? c.BallesRestantes : 1))
                        .ToList()
                )
            );

            TirerSurLesGalinettes(partieDeChasse, timeProvider, repository, builtChasseurs);
            TirerDansLeVide(partieDeChasse, timeProvider, chasseursSansBalles);

            ChangeStatus(partieDeChasse, timeProvider);

            return partieDeChasse;
        }

        private static void TirerDansLeVide(
            PartieDeChasse partieDeChasse,
            Func<DateTime> timeProvider,
            IEnumerable<string> chasseursSansBalles) =>
            chasseursSansBalles.ForEach(c => partieDeChasse.Tirer(c, timeProvider));

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

        private static void ChangeStatus(PartieDeChasse partieDeChasse, PartieStatus status,
            Func<DateTime> timeProvider)
        {
            if (status == PartieStatus.Terminée) partieDeChasse.Terminer(timeProvider);
            else if (status == Apéro) partieDeChasse.PrendreLapéro(timeProvider);
        }
    }
}