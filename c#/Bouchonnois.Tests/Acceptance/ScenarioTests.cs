using Bouchonnois.Service;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;
using FluentAssertions.Extensions;
using static Bouchonnois.Tests.Builders.CommandBuilder;

namespace Bouchonnois.Tests.Acceptance
{
    [UsesVerify]
    public class ScenarioTests
    {
        private DateTime _time = new(2024, 4, 25, 9, 0, 0);
        private readonly PartieDeChasseService _service;

        public ScenarioTests()
        {
            _service = new PartieDeChasseService(
                new PartieDeChasseRepositoryForTests(),
                () => _time
            );
        }

        [Fact]
        public Task DéroulerUnePartie()
        {
            var command = DémarrerUnePartieDeChasse()
                .Avec((Data.Dédé, 20), (Data.Bernard, 8), (Data.Robert, 12))
                .SurUnTerrainRicheEnGalinettes(4);

            var id = _service.Demarrer(
                command.Terrain,
                command.Chasseurs
            );

            After(10.Minutes(), () => _service.Tirer(id, Data.Dédé));
            After(30.Minutes(), () => _service.TirerSurUneGalinette(id, Data.Robert));
            After(20.Minutes(), () => _service.PrendreLapéro(id));
            After(1.Hours(), () => _service.ReprendreLaPartie(id));
            After(2.Minutes(), () => _service.Tirer(id, Data.Bernard));
            After(1.Minutes(), () => _service.Tirer(id, Data.Bernard));
            After(1.Minutes(), () => _service.TirerSurUneGalinette(id, Data.Dédé));
            After(26.Minutes(), () => _service.TirerSurUneGalinette(id, Data.Robert));
            After(10.Minutes(), () => _service.PrendreLapéro(id));
            After(170.Minutes(), () => _service.ReprendreLaPartie(id));
            After(11.Minutes(), () => _service.Tirer(id, Data.Bernard));
            After(1.Seconds(), () => _service.Tirer(id, Data.Bernard));
            After(1.Seconds(), () => _service.Tirer(id, Data.Bernard));
            After(1.Seconds(), () => _service.Tirer(id, Data.Bernard));
            After(1.Seconds(), () => _service.Tirer(id, Data.Bernard));
            After(1.Seconds(), () => _service.Tirer(id, Data.Bernard));
            After(1.Seconds(), () => _service.Tirer(id, Data.Bernard));
            After(19.Minutes(), () => _service.TirerSurUneGalinette(id, Data.Robert));
            After(30.Minutes(), () => _service.TerminerLaPartie(id));

            return Verify(_service.ConsulterStatus(id));
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