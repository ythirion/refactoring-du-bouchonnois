using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;
using Bouchonnois.UseCases;
using FluentAssertions.Extensions;
using static Bouchonnois.Tests.Builders.CommandBuilder;

namespace Bouchonnois.Tests.Acceptance
{
    [UsesVerify]
    public class ScenarioTests
    {
        private DateTime _time = new(2024, 4, 25, 9, 0, 0);

        private readonly DemarrerPartieDeChasse _demarrerPartieDeChasse;
        private readonly Tirer _tirer;
        private readonly TirerSurUneGalinette _tirerSurUneGalinette;
        private readonly PrendreLapéro _prendreLapéro;
        private readonly ReprendreLaPartie _reprendreLaPartie;
        private readonly TerminerLaPartie _terminerLaPartie;
        private readonly ConsulterStatus _consulterStatus;

        public ScenarioTests()
        {
            var repository = new PartieDeChasseRepositoryForTests();

            _demarrerPartieDeChasse = new DemarrerPartieDeChasse(repository, () => _time);
            _tirer = new Tirer(repository, () => _time);
            _tirerSurUneGalinette = new TirerSurUneGalinette(repository, () => _time);
            _prendreLapéro = new PrendreLapéro(repository, () => _time);
            _reprendreLaPartie = new ReprendreLaPartie(repository, () => _time);
            _terminerLaPartie = new TerminerLaPartie(repository, () => _time);
            _consulterStatus = new ConsulterStatus(repository);
        }

        [Fact]
        public Task DéroulerUnePartie()
        {
            var command = DémarrerUnePartieDeChasse()
                .Avec((Data.Dédé, 20), (Data.Bernard, 8), (Data.Robert, 12))
                .SurUnTerrainRicheEnGalinettes(4);

            var id = _demarrerPartieDeChasse.Handle(
                command.Terrain,
                command.Chasseurs
            );

            After(10.Minutes(), () => _tirer.Handle(id, Data.Dédé));
            After(30.Minutes(), () => _tirerSurUneGalinette.Handle(id, Data.Robert));
            After(20.Minutes(), () => _prendreLapéro.Handle(id));
            After(1.Hours(), () => _reprendreLaPartie.Handle(id));
            After(2.Minutes(), () => _tirer.Handle(id, Data.Bernard));
            After(1.Minutes(), () => _tirer.Handle(id, Data.Bernard));
            After(1.Minutes(), () => _tirerSurUneGalinette.Handle(id, Data.Dédé));
            After(26.Minutes(), () => _tirerSurUneGalinette.Handle(id, Data.Robert));
            After(10.Minutes(), () => _prendreLapéro.Handle(id));
            After(170.Minutes(), () => _reprendreLaPartie.Handle(id));
            After(11.Minutes(), () => _tirer.Handle(id, Data.Bernard));
            After(1.Seconds(), () => _tirer.Handle(id, Data.Bernard));
            After(1.Seconds(), () => _tirer.Handle(id, Data.Bernard));
            After(1.Seconds(), () => _tirer.Handle(id, Data.Bernard));
            After(1.Seconds(), () => _tirer.Handle(id, Data.Bernard));
            After(1.Seconds(), () => _tirer.Handle(id, Data.Bernard));
            After(1.Seconds(), () => _tirer.Handle(id, Data.Bernard));
            After(19.Minutes(), () => _tirerSurUneGalinette.Handle(id, Data.Robert));
            After(30.Minutes(), () => _terminerLaPartie.Handle(id));

            return Verify(_consulterStatus.Handle(id));
        }

        private void After(TimeSpan time, Action act)
        {
            _time = _time.Add(time);
            try
            {
                act();
            }
            catch
            {
                // ignored
            }
        }
    }
}