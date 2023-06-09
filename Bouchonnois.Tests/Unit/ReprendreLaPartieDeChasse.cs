using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Unit;

public class ReprendreLaPartieDeChasse : PartieDeChasseServiceTests
{
    [Fact]
    public void QuandLapéroEstEnCours()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
            new List<Chasseur>
            {
                new("Dédé") {BallesRestantes = 20},
                new("Bernard") {BallesRestantes = 8},
                new("Robert") {BallesRestantes = 12},
            }, PartieStatus.Apéro));

        var service = new PartieDeChasseService(repository, TimeProvider);
        service.ReprendreLaPartie(id);

        var savedPartieDeChasse = repository.SavedPartieDeChasse();
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

        AssertLastEvent(repository.SavedPartieDeChasse()!, "Reprise de la chasse");
    }

    public class Echoue
    {
        [Fact]
        public void CarPartieNexistePas()
        {
            var id = Guid.NewGuid();
            var repository = new PartieDeChasseRepositoryForTests();
            var service = new PartieDeChasseService(repository, TimeProvider);
            var reprendrePartieQuandPartieExistePas = () => service.ReprendreLaPartie(id);

            reprendrePartieQuandPartieExistePas.Should()
                .Throw<LaPartieDeChasseNexistePas>();
            repository.SavedPartieDeChasse().Should().BeNull();
        }

        [Fact]
        public void SiLaChasseEstEnCours()
        {
            var id = Guid.NewGuid();
            var repository = new PartieDeChasseRepositoryForTests();

            repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                new List<Chasseur>
                {
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12},
                }));

            var service = new PartieDeChasseService(repository, TimeProvider);
            var reprendreLaPartieQuandChasseEnCours = () => service.ReprendreLaPartie(id);

            reprendreLaPartieQuandChasseEnCours.Should()
                .Throw<LaChasseEstDéjàEnCours>();

            repository.SavedPartieDeChasse().Should().BeNull();
        }

        [Fact]
        public void SiLaPartieDeChasseEstTerminée()
        {
            var id = Guid.NewGuid();
            var repository = new PartieDeChasseRepositoryForTests();

            repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                new List<Chasseur>
                {
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12},
                }, PartieStatus.Terminée));

            var service = new PartieDeChasseService(repository, TimeProvider);
            var prendreLapéroQuandTerminée = () => service.ReprendreLaPartie(id);

            prendreLapéroQuandTerminée.Should()
                .Throw<QuandCestFiniCestFini>();

            repository.SavedPartieDeChasse().Should().BeNull();
        }
    }
}