using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Unit;

public class TirerSurUneGalinette : PartieDeChasseServiceTests
{
    [Fact]
    public void AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain()
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

        service.TirerSurUneGalinette(id, "Bernard");

        var savedPartieDeChasse = repository.SavedPartieDeChasse();
        savedPartieDeChasse!.Id.Should().Be(id);
        savedPartieDeChasse.Status.Should().Be(PartieStatus.EnCours);
        savedPartieDeChasse.Terrain.Nom.Should().Be("Pitibon sur Sauldre");
        savedPartieDeChasse.Terrain.NbGalinettes.Should().Be(2);
        savedPartieDeChasse.Chasseurs.Should().HaveCount(3);
        savedPartieDeChasse.Chasseurs[0].Nom.Should().Be("Dédé");
        savedPartieDeChasse.Chasseurs[0].BallesRestantes.Should().Be(20);
        savedPartieDeChasse.Chasseurs[0].NbGalinettes.Should().Be(0);
        savedPartieDeChasse.Chasseurs[1].Nom.Should().Be("Bernard");
        savedPartieDeChasse.Chasseurs[1].BallesRestantes.Should().Be(7);
        savedPartieDeChasse.Chasseurs[1].NbGalinettes.Should().Be(1);
        savedPartieDeChasse.Chasseurs[2].Nom.Should().Be("Robert");
        savedPartieDeChasse.Chasseurs[2].BallesRestantes.Should().Be(12);
        savedPartieDeChasse.Chasseurs[2].NbGalinettes.Should().Be(0);

        AssertLastEvent(savedPartieDeChasse, "Bernard tire sur une galinette");
    }

    [Fact]
    public void EchoueCarPartieNexistePas()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);
        var tirerQuandPartieExistePas = () => service.TirerSurUneGalinette(id, "Bernard");

        tirerQuandPartieExistePas.Should()
            .Throw<LaPartieDeChasseNexistePas>();
        repository.SavedPartieDeChasse().Should().BeNull();
    }

    [Fact]
    public void EchoueAvecUnChasseurNayantPlusDeBalles()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
            new List<Chasseur>
            {
                new("Dédé") {BallesRestantes = 20},
                new("Bernard") {BallesRestantes = 0},
                new("Robert") {BallesRestantes = 12},
            }));

        var service = new PartieDeChasseService(repository, TimeProvider);
        var tirerSansBalle = () => service.TirerSurUneGalinette(id, "Bernard");

        tirerSansBalle.Should().Throw<TasPlusDeBallesMonVieuxChasseALaMain>();
        AssertLastEvent(repository.SavedPartieDeChasse()!,
            "Bernard veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main");
    }

    [Fact]
    public void EchoueCarPasDeGalinetteSurLeTerrain()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 0},
            new List<Chasseur>
            {
                new("Dédé") {BallesRestantes = 20},
                new("Bernard") {BallesRestantes = 8},
                new("Robert") {BallesRestantes = 12},
            }));

        var service = new PartieDeChasseService(repository, TimeProvider);
        var tirerAlorsQuePasDeGalinettes = () => service.TirerSurUneGalinette(id, "Bernard");

        tirerAlorsQuePasDeGalinettes.Should()
            .Throw<TasTropPicoléMonVieuxTasRienTouché>();
        repository.SavedPartieDeChasse().Should().BeNull();
    }

    [Fact]
    public void EchoueCarLeChasseurNestPasDansLaPartie()
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
        var chasseurInconnuVeutTirer = () => service.TirerSurUneGalinette(id, "Chasseur inconnu");

        chasseurInconnuVeutTirer.Should()
            .Throw<ChasseurInconnu>()
            .WithMessage("Chasseur inconnu Chasseur inconnu");

        repository.SavedPartieDeChasse().Should().BeNull();
    }

    [Fact]
    public void EchoueSiLesChasseursSontEnApero()
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
        var tirerEnPleinApéro = () => service.TirerSurUneGalinette(id, "Chasseur inconnu");

        tirerEnPleinApéro.Should()
            .Throw<OnTirePasPendantLapéroCestSacré>();

        AssertLastEvent(repository.SavedPartieDeChasse()!,
            "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");
    }

    [Fact]
    public void EchoueSiLaPartieDeChasseEstTerminée()
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

        var tirerQuandTerminée = () => service.TirerSurUneGalinette(id, "Chasseur inconnu");

        tirerQuandTerminée.Should()
            .Throw<OnTirePasQuandLaPartieEstTerminée>();

        AssertLastEvent(repository.SavedPartieDeChasse()!,
            "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
    }
}