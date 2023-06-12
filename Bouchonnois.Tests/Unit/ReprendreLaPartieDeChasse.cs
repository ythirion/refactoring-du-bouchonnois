using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class ReprendreLaPartieDeChasse : PartieDeChasseServiceTest
    {
        [Fact]
        public void QuandLapéroEstEnCours()
        {
            var id = Guid.NewGuid();
            Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                new List<Chasseur>
                {
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12},
                }, PartieStatus.Apéro));

            PartieDeChasseService.ReprendreLaPartie(id);

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

            AssertLastEvent(Repository.SavedPartieDeChasse()!, "Reprise de la chasse");
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void CarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var reprendrePartieQuandPartieExistePas = () => PartieDeChasseService.ReprendreLaPartie(id);

                reprendrePartieQuandPartieExistePas.Should()
                    .Throw<LaPartieDeChasseNexistePas>();
                Repository.SavedPartieDeChasse().Should().BeNull();
            }

            [Fact]
            public void SiLaChasseEstEnCours()
            {
                var id = Guid.NewGuid();
                Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20},
                        new("Bernard") {BallesRestantes = 8},
                        new("Robert") {BallesRestantes = 12},
                    }));

                var reprendreLaPartieQuandChasseEnCours = () => PartieDeChasseService.ReprendreLaPartie(id);

                reprendreLaPartieQuandChasseEnCours.Should()
                    .Throw<LaChasseEstDéjàEnCours>();

                Repository.SavedPartieDeChasse().Should().BeNull();
            }

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
            {
                var id = Guid.NewGuid();

                Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20},
                        new("Bernard") {BallesRestantes = 8},
                        new("Robert") {BallesRestantes = 12},
                    }, PartieStatus.Terminée));

                var prendreLapéroQuandTerminée = () => PartieDeChasseService.ReprendreLaPartie(id);

                prendreLapéroQuandTerminée.Should()
                    .Throw<QuandCestFiniCestFini>();

                Repository.SavedPartieDeChasse().Should().BeNull();
            }
        }
    }
}