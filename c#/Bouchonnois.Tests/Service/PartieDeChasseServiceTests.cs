using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Service
{
    public class PartieDeChasseServiceTests
    {
        private static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
        private static readonly Func<DateTime> TimeProvider = () => Now;

        private static void AssertLastEvent(PartieDeChasse partieDeChasse,
            string expectedMessage)
            => partieDeChasse.Events.Should()
                .HaveCount(1)
                .And
                .EndWith(new Event(Now, expectedMessage));

        public class DemarrerUnePartieDeChasse
        {
            [Fact]
            public void AvecPlusieursChasseurs()
            {
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var chasseurs = new List<(string, int)>
                {
                    ("Dédé", 20),
                    ("Bernard", 8),
                    ("Robert", 12)
                };
                var terrainDeChasse = ("Pitibon sur Sauldre", 3);
                var id = service.Demarrer(
                    terrainDeChasse,
                    chasseurs
                );

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

                AssertLastEvent(savedPartieDeChasse,
                    "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)");
            }

            [Fact]
            public void EchoueSansChasseurs()
            {
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var chasseurs = new List<(string, int)>();
                var terrainDeChasse = ("Pitibon sur Sauldre", 3);

                Action demarrerPartieSansChasseurs = () => service.Demarrer(terrainDeChasse, chasseurs);

                demarrerPartieSansChasseurs.Should()
                    .Throw<ImpossibleDeDémarrerUnePartieSansChasseur>();
                repository.SavedPartieDeChasse().Should().BeNull();
            }

            [Fact]
            public void EchoueAvecUnTerrainSansGalinettes()
            {
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var chasseurs = new List<(string, int)>();
                var terrainDeChasse = ("Pitibon sur Sauldre", 0);

                Action demarrerPartieSansChasseurs = () => service.Demarrer(terrainDeChasse, chasseurs);

                demarrerPartieSansChasseurs.Should()
                    .Throw<ImpossibleDeDémarrerUnePartieSansGalinettes>();
            }

            [Fact]
            public void EchoueSiChasseurSansBalle()
            {
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var chasseurs = new List<(string, int)>
                {
                    ("Dédé", 20),
                    ("Bernard", 0),
                };
                var terrainDeChasse = ("Pitibon sur Sauldre", 3);

                Action demarrerPartieAvecChasseurSansBalle = () => service.Demarrer(terrainDeChasse, chasseurs);

                demarrerPartieAvecChasseurSansBalle.Should()
                    .Throw<ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle>();

                repository.SavedPartieDeChasse().Should().BeNull();
            }
        }

        public class TirerSurUneGalinette
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

        public class Tirer
        {
            [Fact]
            public void AvecUnChasseurAyantDesBalles()
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

                service.Tirer(id, "Bernard");

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
                savedPartieDeChasse.Chasseurs[1].BallesRestantes.Should().Be(7);
                savedPartieDeChasse.Chasseurs[1].NbGalinettes.Should().Be(0);
                savedPartieDeChasse.Chasseurs[2].Nom.Should().Be("Robert");
                savedPartieDeChasse.Chasseurs[2].BallesRestantes.Should().Be(12);
                savedPartieDeChasse.Chasseurs[2].NbGalinettes.Should().Be(0);

                AssertLastEvent(repository.SavedPartieDeChasse()!, "Bernard tire");
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var tirerQuandPartieExistePas = () => service.Tirer(id, "Bernard");

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
                var tirerSansBalle = () => service.Tirer(id, "Bernard");

                tirerSansBalle.Should()
                    .Throw<TasPlusDeBallesMonVieuxChasseALaMain>();

                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "Bernard tire -> T'as plus de balles mon vieux, chasse à la main");
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
                var chasseurInconnuVeutTirer = () => service.Tirer(id, "Chasseur inconnu");

                chasseurInconnuVeutTirer
                    .Should()
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
                var tirerEnPleinApéro = () => service.Tirer(id, "Chasseur inconnu");

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
                var tirerQuandTerminée = () => service.Tirer(id, "Chasseur inconnu");

                tirerQuandTerminée.Should()
                    .Throw<OnTirePasQuandLaPartieEstTerminée>();

                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
            }
        }

        public class PrendreLApéro
        {
            [Fact]
            public void QuandLaPartieEstEnCours()
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
                service.PrendreLapéro(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse!.Id.Should().Be(id);
                savedPartieDeChasse.Status.Should().Be(PartieStatus.Apéro);
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

                AssertLastEvent(repository.SavedPartieDeChasse()!, "Petit apéro");
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var apéroQuandPartieExistePas = () => service.PrendreLapéro(id);

                apéroQuandPartieExistePas.Should()
                    .Throw<LaPartieDeChasseNexistePas>();
                repository.SavedPartieDeChasse().Should().BeNull();
            }

            [Fact]
            public void EchoueSiLesChasseursSontDéjaEnApero()
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
                var prendreLApéroQuandOnPrendDéjàLapéro = () => service.PrendreLapéro(id);

                prendreLApéroQuandOnPrendDéjàLapéro.Should()
                    .Throw<OnEstDéjàEnTrainDePrendreLapéro>();
                repository.SavedPartieDeChasse().Should().BeNull();
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
                var prendreLapéroQuandTerminée = () => service.PrendreLapéro(id);

                prendreLapéroQuandTerminée.Should()
                    .Throw<OnPrendPasLapéroQuandLaPartieEstTerminée>();
                repository.SavedPartieDeChasse().Should().BeNull();
            }
        }

        public class ReprendreLaPartieDeChasse
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

            [Fact]
            public void EchoueCarPartieNexistePas()
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
            public void EchoueSiLaChasseEstEnCours()
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
                var prendreLapéroQuandTerminée = () => service.ReprendreLaPartie(id);

                prendreLapéroQuandTerminée.Should()
                    .Throw<QuandCestFiniCestFini>();

                repository.SavedPartieDeChasse().Should().BeNull();
            }
        }

        public class TerminerLaPartieDeChasse
        {
            [Fact]
            public void QuandLaPartieEstEnCoursEt1SeulChasseurGagne()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20},
                        new("Bernard") {BallesRestantes = 8},
                        new("Robert") {BallesRestantes = 12, NbGalinettes = 2},
                    }));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse!.Id.Should().Be(id);
                savedPartieDeChasse.Status.Should().Be(PartieStatus.Terminée);
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
                savedPartieDeChasse.Chasseurs[2].NbGalinettes.Should().Be(2);

                meilleurChasseur.Should().Be("Robert");
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");
            }

            [Fact]
            public void QuandLaPartieEstEnCoursEt1SeulChasseurDansLaPartie()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Robert") {BallesRestantes = 12, NbGalinettes = 2},
                    }));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse!.Id.Should().Be(id);
                savedPartieDeChasse.Status.Should().Be(PartieStatus.Terminée);
                savedPartieDeChasse.Terrain.Nom.Should().Be("Pitibon sur Sauldre");
                savedPartieDeChasse.Terrain.NbGalinettes.Should().Be(3);
                savedPartieDeChasse.Chasseurs.Should().HaveCount(1);
                savedPartieDeChasse.Chasseurs[0].Nom.Should().Be("Robert");
                savedPartieDeChasse.Chasseurs[0].BallesRestantes.Should().Be(12);
                savedPartieDeChasse.Chasseurs[0].NbGalinettes.Should().Be(2);

                meilleurChasseur.Should().Be("Robert");
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");
            }

            [Fact]
            public void QuandLaPartieEstEnCoursEt2ChasseursExAequo()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20, NbGalinettes = 2},
                        new("Bernard") {BallesRestantes = 8, NbGalinettes = 2},
                        new("Robert") {BallesRestantes = 12},
                    }));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse!.Id.Should().Be(id);
                savedPartieDeChasse.Status.Should().Be(PartieStatus.Terminée);
                savedPartieDeChasse.Terrain.Nom.Should().Be("Pitibon sur Sauldre");
                savedPartieDeChasse.Terrain.NbGalinettes.Should().Be(3);
                savedPartieDeChasse.Chasseurs.Should().HaveCount(3);
                savedPartieDeChasse.Chasseurs[0].Nom.Should().Be("Dédé");
                savedPartieDeChasse.Chasseurs[0].BallesRestantes.Should().Be(20);
                savedPartieDeChasse.Chasseurs[0].NbGalinettes.Should().Be(2);
                savedPartieDeChasse.Chasseurs[1].Nom.Should().Be("Bernard");
                savedPartieDeChasse.Chasseurs[1].BallesRestantes.Should().Be(8);
                savedPartieDeChasse.Chasseurs[1].NbGalinettes.Should().Be(2);
                savedPartieDeChasse.Chasseurs[2].Nom.Should().Be("Robert");
                savedPartieDeChasse.Chasseurs[2].BallesRestantes.Should().Be(12);
                savedPartieDeChasse.Chasseurs[2].NbGalinettes.Should().Be(0);

                meilleurChasseur.Should().Be("Dédé, Bernard");
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "La partie de chasse est terminée, vainqueur : Dédé - 2 galinettes, Bernard - 2 galinettes");
            }

            [Fact]
            public void QuandLaPartieEstEnCoursEtToutLeMondeBrocouille()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse(
                    id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20},
                        new("Bernard") {BallesRestantes = 8},
                        new("Robert") {BallesRestantes = 12},
                    }
                    , new List<Event>()
                ));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse!.Id.Should().Be(id);
                savedPartieDeChasse.Status.Should().Be(PartieStatus.Terminée);
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

                meilleurChasseur.Should().Be("Brocouille");
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "La partie de chasse est terminée, vainqueur : Brocouille");
            }

            [Fact]
            public void QuandLesChasseursSontALaperoEtTousExAequo()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20, NbGalinettes = 3},
                        new("Bernard") {BallesRestantes = 8, NbGalinettes = 3},
                        new("Robert") {BallesRestantes = 12, NbGalinettes = 3},
                    }, PartieStatus.Apéro));

                var service = new PartieDeChasseService(repository, TimeProvider);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse!.Id.Should().Be(id);
                savedPartieDeChasse.Status.Should().Be(PartieStatus.Terminée);
                savedPartieDeChasse.Terrain.Nom.Should().Be("Pitibon sur Sauldre");
                savedPartieDeChasse.Terrain.NbGalinettes.Should().Be(3);
                savedPartieDeChasse.Chasseurs.Should().HaveCount(3);
                savedPartieDeChasse.Chasseurs[0].Nom.Should().Be("Dédé");
                savedPartieDeChasse.Chasseurs[0].BallesRestantes.Should().Be(20);
                savedPartieDeChasse.Chasseurs[0].NbGalinettes.Should().Be(3);
                savedPartieDeChasse.Chasseurs[1].Nom.Should().Be("Bernard");
                savedPartieDeChasse.Chasseurs[1].BallesRestantes.Should().Be(8);
                savedPartieDeChasse.Chasseurs[1].NbGalinettes.Should().Be(3);
                savedPartieDeChasse.Chasseurs[2].Nom.Should().Be("Robert");
                savedPartieDeChasse.Chasseurs[2].BallesRestantes.Should().Be(12);
                savedPartieDeChasse.Chasseurs[2].NbGalinettes.Should().Be(3);

                meilleurChasseur.Should().Be("Dédé, Bernard, Robert");
                AssertLastEvent(repository.SavedPartieDeChasse()!,
                    "La partie de chasse est terminée, vainqueur : Dédé - 3 galinettes, Bernard - 3 galinettes, Robert - 3 galinettes");
            }

            [Fact]
            public void EchoueSiLaPartieDeChasseEstDéjàTerminée()
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
                var prendreLapéroQuandTerminée = () => service.TerminerLaPartie(id);

                prendreLapéroQuandTerminée.Should()
                    .Throw<QuandCestFiniCestFini>();

                repository.SavedPartieDeChasse().Should().BeNull();
            }
        }

        public class ConsulterStatus
        {
            [Fact]
            public void QuandLaPartieVientDeDémarrer()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);

                repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre")
                {
                    NbGalinettes = 3
                }, new List<Chasseur>
                {
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12, NbGalinettes = 2},
                }, new List<Event>
                {
                    new(new DateTime(2024, 4, 25, 9, 0, 12),
                        "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)")
                }));

                var status = service.ConsulterStatus(id);

                status.Should()
                    .Be(
                        "09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
                    );
            }

            [Fact]
            public void QuandLaPartieEstTerminée()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);

                repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20},
                        new("Bernard") {BallesRestantes = 8},
                        new("Robert") {BallesRestantes = 12, NbGalinettes = 2},
                    }, new List<Event>
                    {
                        new(new DateTime(2024, 4, 25, 9, 0, 12),
                            "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"),
                        new(new DateTime(2024, 4, 25, 9, 10, 0), "Dédé tire"),
                        new(new DateTime(2024, 4, 25, 9, 40, 0), "Robert tire sur une galinette"),
                        new(new DateTime(2024, 4, 25, 10, 0, 0), "Petit apéro"),
                        new(new DateTime(2024, 4, 25, 11, 0, 0), "Reprise de la chasse"),
                        new(new DateTime(2024, 4, 25, 11, 2, 0), "Bernard tire"),
                        new(new DateTime(2024, 4, 25, 11, 3, 0), "Bernard tire"),
                        new(new DateTime(2024, 4, 25, 11, 4, 0), "Dédé tire sur une galinette"),
                        new(new DateTime(2024, 4, 25, 11, 30, 0), "Robert tire sur une galinette"),
                        new(new DateTime(2024, 4, 25, 11, 40, 0), "Petit apéro"),
                        new(new DateTime(2024, 4, 25, 14, 30, 0), "Reprise de la chasse"),
                        new(new DateTime(2024, 4, 25, 14, 41, 0), "Bernard tire"),
                        new(new DateTime(2024, 4, 25, 14, 41, 1), "Bernard tire"),
                        new(new DateTime(2024, 4, 25, 14, 41, 2), "Bernard tire"),
                        new(new DateTime(2024, 4, 25, 14, 41, 3), "Bernard tire"),
                        new(new DateTime(2024, 4, 25, 14, 41, 4), "Bernard tire"),
                        new(new DateTime(2024, 4, 25, 14, 41, 5), "Bernard tire"),
                        new(new DateTime(2024, 4, 25, 14, 41, 6), "Bernard tire"),
                        new(new DateTime(2024, 4, 25, 14, 41, 7),
                            "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"),
                        new(new DateTime(2024, 4, 25, 15, 0, 0), "Robert tire sur une galinette"),
                        new(new DateTime(2024, 4, 25, 15, 30, 0),
                            "La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes"),
                    }));

                var status = service.ConsulterStatus(id);

                status.Should()
                    .BeEquivalentTo(
                        @"15:30 - La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes
15:00 - Robert tire sur une galinette
14:41 - Bernard tire -> T'as plus de balles mon vieux, chasse à la main
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:30 - Reprise de la chasse
11:40 - Petit apéro
11:30 - Robert tire sur une galinette
11:04 - Dédé tire sur une galinette
11:03 - Bernard tire
11:02 - Bernard tire
11:00 - Reprise de la chasse
10:00 - Petit apéro
09:40 - Robert tire sur une galinette
09:10 - Dédé tire
09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
                    );
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, TimeProvider);
                var reprendrePartieQuandPartieExistePas = () => service.ConsulterStatus(id);

                reprendrePartieQuandPartieExistePas.Should()
                    .Throw<LaPartieDeChasseNexistePas>();
                repository.SavedPartieDeChasse().Should().BeNull();
            }
        }
    }
}