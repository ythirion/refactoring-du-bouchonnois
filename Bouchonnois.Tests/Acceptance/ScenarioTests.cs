using Bouchonnois.Domain.Commands;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;
using Bouchonnois.UseCases;
using FluentAssertions.Extensions;
using static Bouchonnois.Tests.Builders.CommandBuilder;
using ConsulterStatus = Bouchonnois.Domain.Commands.ConsulterStatus;
using DemarrerPartieDeChasse = Bouchonnois.Domain.Commands.DemarrerPartieDeChasse;

namespace Bouchonnois.Tests.Acceptance
{
    [UsesVerify]
    public class ScenarioTests
    {
        private DateTime _time = new(2024, 4, 25, 9, 0, 0);

        private readonly UseCases.DemarrerPartieDeChasse _demarrerPartieDeChasse;
        private readonly Tirer _tirer;
        private readonly TirerSurUneGalinette _tirerSurUneGalinette;
        private readonly PrendreLapéro _prendreLapéro;
        private readonly ReprendreLaPartie _reprendreLaPartie;
        private readonly TerminerLaPartie _terminerLaPartie;
        private readonly UseCases.ConsulterStatus _consulterStatus;

        public ScenarioTests()
        {
            var repository = new PartieDeChasseRepositoryForTests();
            var timeProvider = () => _time;

            _demarrerPartieDeChasse = new UseCases.DemarrerPartieDeChasse(repository, timeProvider);
            _tirer = new Tirer(repository, timeProvider);
            _tirerSurUneGalinette = new TirerSurUneGalinette(repository, timeProvider);
            _prendreLapéro = new PrendreLapéro(repository, timeProvider);
            _reprendreLaPartie = new ReprendreLaPartie(repository, timeProvider);
            _terminerLaPartie = new TerminerLaPartie(repository, timeProvider);
            _consulterStatus = new UseCases.ConsulterStatus(repository);
        }

        [Fact]
        public Task DéroulerUnePartie()
        {
            var command = DémarrerUnePartieDeChasse()
                .Avec((Data.Dédé, 20), (Data.Bernard, 8), (Data.Robert, 12))
                .SurUnTerrainRicheEnGalinettes(4);

            var id = _demarrerPartieDeChasse.Handle(command.Build());

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

            return Verify(_consulterStatus.Handle(new ConsulterStatus(id)));
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