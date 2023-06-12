using Bouchonnois.Domain;
using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class DemarrerUnePartieDeChasse : PartieDeChasseServiceTest
    {
        [Fact]
        public void AvecPlusieursChasseurs()
        {
            var chasseurs = new List<(string, int)>
            {
                ("Dédé", 20),
                ("Bernard", 8),
                ("Robert", 12)
            };
            var terrainDeChasse = ("Pitibon sur Sauldre", 3);
            var id = PartieDeChasseService.Demarrer(
                terrainDeChasse,
                chasseurs
            );

            var savedPartieDeChasse = Repository.SavedPartieDeChasse();
            savedPartieDeChasse!.Id.Should().Be(id);
            savedPartieDeChasse.Status.Should().Be(PartieStatus.EnCours);
            savedPartieDeChasse.Terrain.Nom.Should().Be("Pitibon sur Sauldre");
            savedPartieDeChasse.Terrain.NbGalinettes.Should().Be(3);
            savedPartieDeChasse.Chasseurs.Should().HaveCount(3);
            savedPartieDeChasse.Chasseurs[0].Nom.Should().Be("Dédé");
            savedPartieDeChasse.Chasseurs[0].BallesRestantes.Should().Be(20);
            savedPartieDeChasse.Chasseurs[0].NbGalinettes.Should().Be(0);
            savedPartieDeChasse.Chasseurs[1].Nom.Should().Be("Bernard");
            savedPartieDeChasse.Chasseurs[1].BallesRestantes.Should().Be(8);
            savedPartieDeChasse.Chasseurs[1].NbGalinettes.Should().Be(0);
            savedPartieDeChasse.Chasseurs[2].Nom.Should().Be("Robert");
            savedPartieDeChasse.Chasseurs[2].BallesRestantes.Should().Be(12);
            savedPartieDeChasse.Chasseurs[2].NbGalinettes.Should().Be(0);

            AssertLastEvent(savedPartieDeChasse,
                "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)");
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void SansChasseurs()
            {
                var chasseurs = new List<(string, int)>();
                var terrainDeChasse = ("Pitibon sur Sauldre", 3);

                Action demarrerPartieSansChasseurs = () => PartieDeChasseService.Demarrer(terrainDeChasse, chasseurs);

                demarrerPartieSansChasseurs.Should()
                    .Throw<ImpossibleDeDémarrerUnePartieSansChasseur>();
                Repository.SavedPartieDeChasse().Should().BeNull();
            }

            [Fact]
            public void AvecUnTerrainSansGalinettes()
            {
                var chasseurs = new List<(string, int)>();
                var terrainDeChasse = ("Pitibon sur Sauldre", 0);

                Action demarrerPartieSansChasseurs = () => PartieDeChasseService.Demarrer(terrainDeChasse, chasseurs);

                demarrerPartieSansChasseurs.Should()
                    .Throw<ImpossibleDeDémarrerUnePartieSansGalinettes>();
            }

            [Fact]
            public void SiChasseurSansBalle()
            {
                var chasseurs = new List<(string, int)>
                {
                    ("Dédé", 20),
                    ("Bernard", 0),
                };
                var terrainDeChasse = ("Pitibon sur Sauldre", 3);

                Action demarrerPartieAvecChasseurSansBalle =
                    () => PartieDeChasseService.Demarrer(terrainDeChasse, chasseurs);

                demarrerPartieAvecChasseurSansBalle.Should()
                    .Throw<ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle>();

                Repository.SavedPartieDeChasse().Should().BeNull();
            }
        }
    }
}