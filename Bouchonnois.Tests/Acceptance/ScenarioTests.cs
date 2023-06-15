using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;
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

            _time = _time.Add(TimeSpan.FromMinutes(10));
            _service.Tirer(id, Data.Dédé);

            _time = _time.Add(TimeSpan.FromMinutes(30));
            _service.TirerSurUneGalinette(id, Data.Robert);

            _time = _time.Add(TimeSpan.FromMinutes(20));
            _service.PrendreLapéro(id);

            _time = _time.Add(TimeSpan.FromHours(1));
            _service.ReprendreLaPartie(id);

            _time = _time.Add(TimeSpan.FromMinutes(2));
            _service.Tirer(id, Data.Bernard);

            _time = _time.Add(TimeSpan.FromMinutes(1));
            _service.Tirer(id, Data.Bernard);

            _time = _time.Add(TimeSpan.FromMinutes(1));
            _service.TirerSurUneGalinette(id, Data.Dédé);

            _time = _time.Add(TimeSpan.FromMinutes(26));
            _service.TirerSurUneGalinette(id, Data.Robert);

            _time = _time.Add(TimeSpan.FromMinutes(10));
            _service.PrendreLapéro(id);

            _time = _time.Add(TimeSpan.FromMinutes(170));
            _service.ReprendreLaPartie(id);

            _time = _time.Add(TimeSpan.FromMinutes(11));
            _service.Tirer(id, Data.Bernard);

            _time = _time.Add(TimeSpan.FromSeconds(1));
            _service.Tirer(id, Data.Bernard);

            _time = _time.Add(TimeSpan.FromSeconds(1));
            _service.Tirer(id, Data.Bernard);

            _time = _time.Add(TimeSpan.FromSeconds(1));
            _service.Tirer(id, Data.Bernard);

            _time = _time.Add(TimeSpan.FromSeconds(1));
            _service.Tirer(id, Data.Bernard);

            _time = _time.Add(TimeSpan.FromSeconds(1));
            _service.Tirer(id, Data.Bernard);

            _time = _time.Add(TimeSpan.FromSeconds(1));

            try
            {
                _service.Tirer(id, Data.Bernard);
            }
            catch (TasPlusDeBallesMonVieuxChasseALaMain)
            {
            }

            _time = _time.Add(TimeSpan.FromMinutes(19));
            _service.TirerSurUneGalinette(id, Data.Robert);

            _time = _time.Add(TimeSpan.FromMinutes(30));
            _service.TerminerLaPartie(id);

            return Verify(_service.ConsulterStatus(id));
        }
    }
}