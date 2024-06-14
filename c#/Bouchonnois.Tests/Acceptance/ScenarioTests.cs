using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;
using Bouchonnois.UseCases;
using Domain.Core;
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
            DateTime TimeProvider() => _time;
            var repository = new PartieDeChasseRepositoryForTests(new InMemoryEventStore(TimeProvider));

            _demarrerPartieDeChasse = new DemarrerPartieDeChasse(repository, TimeProvider);
            _tirer = new Tirer(repository);
            _tirerSurUneGalinette = new TirerSurUneGalinette(repository);
            _prendreLapéro = new PrendreLapéro(repository);
            _reprendreLaPartie = new ReprendreLaPartie(repository);
            _terminerLaPartie = new TerminerLaPartie(repository);
            _consulterStatus = new ConsulterStatus(repository);
        }

        [Fact]
        public async Task DéroulerUnePartie()
        {
            var command = DémarrerUnePartieDeChasse()
                .Avec((Data.Dédé, 20), (Data.Bernard, 8), (Data.Robert, 12))
                .SurUnTerrainRicheEnGalinettes(4);

            var id = (await _demarrerPartieDeChasse.Handle(command.Build())).RightUnsafe();

            AfterSync(10.Minutes(), async () => await _tirer.Handle(new Domain.Tirer.Tirer(id, Data.Dédé)));
            AfterSync(30.Minutes(),
                async () => await _tirerSurUneGalinette.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.Robert)));
            AfterSync(20.Minutes(), async () => await _prendreLapéro.Handle(new Domain.Apéro.PrendreLapéro(id)));
            AfterSync(1.Hours(),
                async () => await _reprendreLaPartie.Handle(new Domain.Reprendre.ReprendreLaPartie(id)));
            AfterSync(2.Minutes(), async () => await _tirer.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));
            AfterSync(1.Minutes(), async () => await _tirer.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));
            AfterSync(1.Minutes(),
                async () => await _tirerSurUneGalinette.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.Dédé)));
            AfterSync(26.Minutes(),
                async () => await _tirerSurUneGalinette.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.Robert)));
            AfterSync(10.Minutes(), async () => await _prendreLapéro.Handle(new Domain.Apéro.PrendreLapéro(id)));
            AfterSync(170.Minutes(),
                async () => await _reprendreLaPartie.Handle(new Domain.Reprendre.ReprendreLaPartie(id)));
            AfterSync(11.Minutes(), async () => await _tirer.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));
            AfterSync(1.Seconds(), async () => await _tirer.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));
            AfterSync(1.Seconds(), async () => await _tirer.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));
            AfterSync(1.Seconds(), async () => await _tirer.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));
            AfterSync(1.Seconds(), async () => await _tirer.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));
            AfterSync(1.Seconds(), async () => await _tirer.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));
            AfterSync(1.Seconds(), async () => await _tirer.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));
            AfterSync(19.Minutes(),
                async () => await _tirerSurUneGalinette.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.Robert)));
            AfterSync(30.Minutes(),
                async () => await _terminerLaPartie.Handle(new Domain.Terminer.TerminerLaPartie(id)));

            var status = await _consulterStatus.Handle(new Domain.Consulter.ConsulterStatus(id));

            await Verify(status.RightUnsafe());
        }

        private void AfterSync(TimeSpan time, Func<Task> act)
            => AsyncHelper.RunSync(() => After(time, act));

        private async Task After(TimeSpan time, Func<Task> act)
        {
            _time = _time.Add(time);
            await act();
        }
    }
}