using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Doubles;
using static Bouchonnois.Tests.Builders.ChasseurBuilder;
using static Bouchonnois.Tests.Builders.PartieDeChasseBuilder;

namespace Bouchonnois.Tests.Unit
{
    public class TirerSurUneGalinette : PartieDeChasseServiceTest
    {
        [Fact]
        public void AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain()
        {
            var partieDeChasse = AvecUnePartieDeChasseExistante(
                UnePartieDeChasseDuBouchonnois()
                    .SurUnTerrainRicheEnGalinettes()
                    .Avec(Dédé(), Bernard(), Robert())
            );

            PartieDeChasseService.TirerSurUneGalinette(partieDeChasse.Id, "Bernard");

            SavedPartieDeChasse()
                .Should()
                .HaveEmittedEvent(Now, "Bernard tire sur une galinette").And
                .ChasseurATiréSurUneGalinette("Bernard", 7, 1).And
                .GalinettesSurLeTerrain(2);
        }

        public class Echoue
        {
            [Fact]
            public void CarPartieNexistePas()
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
            public void AvecUnChasseurNayantPlusDeBalles()
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
            public void CarPasDeGalinetteSurLeTerrain()
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
            public void CarLeChasseurNestPasDansLaPartie()
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
            public void SiLesChasseursSontEnApero()
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

                var tirerQuandTerminée = () => service.TirerSurUneGalinette(id, "Chasseur inconnu");

                tirerQuandTerminée.Should()
                    .Throw<OnTirePasQuandLaPartieEstTerminée>();

                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
            }
        }
    }
}