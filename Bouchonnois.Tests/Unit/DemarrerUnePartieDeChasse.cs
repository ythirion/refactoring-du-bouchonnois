using Bouchonnois.Domain;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Builders;
using static Bouchonnois.Tests.Builders.CommandBuilder;

namespace Bouchonnois.Tests.Unit
{
    public class DemarrerUnePartieDeChasse : PartieDeChasseServiceTest
    {
        [Fact]
        public void AvecPlusieursChasseurs()
        {
            var command = DémarrerUnePartieDeChasse()
                .Avec((Data.Dédé, 20), (Data.Bernard, 8), (Data.Robert, 12))
                .SurUnTerrainRicheEnGalinettes();

            var id = PartieDeChasseService.Demarrer(
                command.Terrain,
                command.Chasseurs
            );

            var savedPartieDeChasse = Repository.SavedPartieDeChasse();
            savedPartieDeChasse!.Id.Should().Be(id);
            savedPartieDeChasse.Status.Should().Be(PartieStatus.EnCours);
            savedPartieDeChasse.Terrain.Nom.Should().Be("Pitibon sur Sauldre");
            savedPartieDeChasse.Terrain.NbGalinettes.Should().Be(3);
            savedPartieDeChasse.Chasseurs.Should().HaveCount(3);
            savedPartieDeChasse.Chasseurs[0].Nom.Should().Be(Data.Dédé);
            savedPartieDeChasse.Chasseurs[0].BallesRestantes.Should().Be(20);
            savedPartieDeChasse.Chasseurs[0].NbGalinettes.Should().Be(0);
            savedPartieDeChasse.Chasseurs[1].Nom.Should().Be(Data.Bernard);
            savedPartieDeChasse.Chasseurs[1].BallesRestantes.Should().Be(8);
            savedPartieDeChasse.Chasseurs[1].NbGalinettes.Should().Be(0);
            savedPartieDeChasse.Chasseurs[2].Nom.Should().Be(Data.Robert);
            savedPartieDeChasse.Chasseurs[2].BallesRestantes.Should().Be(12);
            savedPartieDeChasse.Chasseurs[2].NbGalinettes.Should().Be(0);

            savedPartieDeChasse.Should()
                .HaveEmittedEvent(Now,
                    "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)");
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