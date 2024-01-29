using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Builders;
using static Bouchonnois.Tests.Builders.CommandBuilder;

namespace Bouchonnois.Tests.Unit
{
    [UsesVerify]
    public class DemarrerUnePartieDeChasse : PartieDeChasseServiceTest
    {
        [Fact]
        public Task AvecPlusieursChasseurs()
        {
            var command = DémarrerUnePartieDeChasse()
                .Avec((Data.Dédé, 20), (Data.Bernard, 8), (Data.Robert, 12))
                .SurUnTerrainRicheEnGalinettes();

            PartieDeChasseService.Demarrer(
                command.Terrain,
                command.Chasseurs
            );

            return Verify(Repository.SavedPartieDeChasse())
                .DontScrubDateTimes();
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void SansChasseurs()
                => ExecuteAndAssertThrow<ImpossibleDeDémarrerUnePartieSansChasseur>(
                    s => s.Demarrer(("Pitibon sur Sauldre", 3), new List<(string, int)>()),
                    p => p.Should().BeNull()
                );

            [Fact]
            public void AvecUnTerrainSansGalinettes()
            {
                Action demarrerPartieSansChasseurs = () =>
                    PartieDeChasseService.Demarrer(("Pitibon sur Sauldre", 0), new List<(string, int)>());

                demarrerPartieSansChasseurs.Should()
                    .Throw<ImpossibleDeDémarrerUnePartieSansGalinettes>();
            }

            [Fact]
            public void SiChasseurSansBalle()
            {
                var command = DémarrerUnePartieDeChasse()
                    .Avec((Data.Dédé, 20), (Data.Bernard, 0))
                    .SurUnTerrainRicheEnGalinettes();

                ExecuteAndAssertThrow<ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle>(
                    s => s.Demarrer(command.Terrain, command.Chasseurs),
                    p => p.Should().BeNull());
            }
        }
    }
}