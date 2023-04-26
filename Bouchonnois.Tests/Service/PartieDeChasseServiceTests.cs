using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Service
{
    public class PartieDeChasseServiceTests
    {
        public class DemarrerUnePartieDeChasse
        {
            [Fact]
            public void AvecPlusieursChasseurs()
            {
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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
                savedPartieDeChasse.Id.Should().Be(id);
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
            }

            [Fact]
            public void EchoueSansChasseurs()
            {
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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
                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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
                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);

                service.TirerSurUneGalinette(id, "Bernard");

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse.Id.Should().Be(id);
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
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 0},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var tirerSansBalle = () => service.TirerSurUneGalinette(id, "Bernard");

                tirerSansBalle.Should()
                    .Throw<TasPlusDeBallesMonVieuxChasseALaMain>();
            }

            [Fact]
            public void EchoueCarPasDeGalinetteSurLeTerrain()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 0
                    },
                    Status = PartieStatus.EnCours
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var chasseurInconnuVeutTirer = () => service.TirerSurUneGalinette(id, "Chasseur inconnu");

                chasseurInconnuVeutTirer.Should()
                    .Throw<ChasseurInconnu>();
                repository.SavedPartieDeChasse().Should().BeNull();
            }

            [Fact]
            public void EchoueSiLesChasseursSontEnApero()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.Apéro,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var tirerEnPleinApéro = () => service.TirerSurUneGalinette(id, "Chasseur inconnu");

                tirerEnPleinApéro.Should()
                    .Throw<OnTirePasPendantLapéroCestSacré>();
            }

            [Fact]
            public void EchoueSiLaPartieDeChasseEstTerminée()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.Terminée,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var tirerQuandTerminée = () => service.TirerSurUneGalinette(id, "Chasseur inconnu");

                tirerQuandTerminée.Should()
                    .Throw<OnTirePasQuandLaPartieEstTerminée>();
            }
        }

        public class Tirer
        {
            [Fact]
            public void AvecUnChasseurAyantDesBalles()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);

                service.Tirer(id, "Bernard");

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse.Id.Should().Be(id);
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
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 0},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var tirerSansBalle = () => service.Tirer(id, "Bernard");

                tirerSansBalle.Should()
                    .Throw<TasPlusDeBallesMonVieuxChasseALaMain>();
            }

            [Fact]
            public void EchoueCarLeChasseurNestPasDansLaPartie()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var chasseurInconnuVeutTirer = () => service.Tirer(id, "Chasseur inconnu");

                chasseurInconnuVeutTirer.Should()
                    .Throw<ChasseurInconnu>();
                repository.SavedPartieDeChasse().Should().BeNull();
            }

            [Fact]
            public void EchoueSiLesChasseursSontEnApero()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.Apéro,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var tirerEnPleinApéro = () => service.Tirer(id, "Chasseur inconnu");

                tirerEnPleinApéro.Should()
                    .Throw<OnTirePasPendantLapéroCestSacré>();
            }

            [Fact]
            public void EchoueSiLaPartieDeChasseEstTerminée()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.Terminée,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var tirerQuandTerminée = () => service.Tirer(id, "Chasseur inconnu");

                tirerQuandTerminée.Should()
                    .Throw<OnTirePasQuandLaPartieEstTerminée>();
            }
        }

        public class PrendreLApéro
        {
            [Fact]
            public void QuandLaPartieEstEnCours()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                service.PrendreLapéro(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse.Id.Should().Be(id);
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
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.Apéro
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.Terminée
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.Apéro,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                service.ReprendreLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse.Id.Should().Be(id);
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
            }

            [Fact]
            public void EchoueCarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();
                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.Terminée
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12, NbGalinettes = 2},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse.Id.Should().Be(id);
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
            }

            [Fact]
            public void QuandLaPartieEstEnCoursEt1SeulChasseurDansLaPartie()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Robert", BallesRestantes = 12, NbGalinettes = 2}
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse.Id.Should().Be(id);
                savedPartieDeChasse.Status.Should().Be(PartieStatus.Terminée);
                savedPartieDeChasse.Terrain.Nom.Should().Be("Pitibon sur Sauldre");
                savedPartieDeChasse.Terrain.NbGalinettes.Should().Be(3);
                savedPartieDeChasse.Chasseurs.Should().HaveCount(1);
                savedPartieDeChasse.Chasseurs[0].Nom.Should().Be("Robert");
                savedPartieDeChasse.Chasseurs[0].BallesRestantes.Should().Be(12);
                savedPartieDeChasse.Chasseurs[0].NbGalinettes.Should().Be(2);

                meilleurChasseur.Should().Be("Robert");
            }

            [Fact]
            public void QuandLaPartieEstEnCoursEt2ChasseursExAequo()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20, NbGalinettes = 2},
                        new() {Nom = "Bernard", BallesRestantes = 8, NbGalinettes = 2},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse.Id.Should().Be(id);
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
            }

            [Fact]
            public void QuandLaPartieEstEnCoursEtToutLeMondeBrocouille()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse.Id.Should().Be(id);
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
            }

            [Fact]
            public void QuandLesChasseursSontALaperoEtTousExAequo()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20, NbGalinettes = 3},
                        new() {Nom = "Bernard", BallesRestantes = 8, NbGalinettes = 3},
                        new() {Nom = "Robert", BallesRestantes = 12, NbGalinettes = 3}
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.Apéro,
                    Events = new List<Event>()
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var meilleurChasseur = service.TerminerLaPartie(id);

                var savedPartieDeChasse = repository.SavedPartieDeChasse();
                savedPartieDeChasse.Id.Should().Be(id);
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
            }

            [Fact]
            public void EchoueSiLaPartieDeChasseEstDéjàTerminée()
            {
                var id = Guid.NewGuid();
                var repository = new PartieDeChasseRepositoryForTests();

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.Terminée
                });

                var service = new PartieDeChasseService(repository, () => DateTime.Now);
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
                var service = new PartieDeChasseService(repository, () => DateTime.Now);

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12, NbGalinettes = 2},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>
                    {
                        new(new DateTime(2024, 4, 25, 9, 0, 12),
                            "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)")
                    }
                });

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
                var service = new PartieDeChasseService(repository, () => DateTime.Now);

                repository.Add(new PartieDeChasse
                {
                    Id = id,
                    Chasseurs = new List<Chasseur>
                    {
                        new() {Nom = "Dédé", BallesRestantes = 20},
                        new() {Nom = "Bernard", BallesRestantes = 8},
                        new() {Nom = "Robert", BallesRestantes = 12, NbGalinettes = 2},
                    },
                    Terrain = new Terrain
                    {
                        Nom = "Pitibon sur Sauldre",
                        NbGalinettes = 3
                    },
                    Status = PartieStatus.EnCours,
                    Events = new List<Event>
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
                    }
                });

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
                var service = new PartieDeChasseService(repository, () => DateTime.Now);
                var reprendrePartieQuandPartieExistePas = () => service.ConsulterStatus(id);

                reprendrePartieQuandPartieExistePas.Should()
                    .Throw<LaPartieDeChasseNexistePas>();
                repository.SavedPartieDeChasse().Should().BeNull();
            }
        }
    }
}