using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Unit
{
    public class Tirer : PartieDeChasseServiceTest
    {
        [Fact]
        public void AvecUnChasseurAyantDesBalles()
        {
            var id = Guid.NewGuid();
            Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                new List<Chasseur>
                {
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12},
                }));

            PartieDeChasseService.Tirer(id, "Bernard");

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
            savedPartieDeChasse.Chasseurs[1].BallesRestantes.Should().Be(7);
            savedPartieDeChasse.Chasseurs[1].NbGalinettes.Should().Be(0);
            savedPartieDeChasse.Chasseurs[2].Nom.Should().Be("Robert");
            savedPartieDeChasse.Chasseurs[2].BallesRestantes.Should().Be(12);
            savedPartieDeChasse.Chasseurs[2].NbGalinettes.Should().Be(0);

            AssertLastEvent(Repository.SavedPartieDeChasse()!, "Bernard tire");
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void CarPartieNexistePas()
            {
                var id = Guid.NewGuid();
                var tirerQuandPartieExistePas = () => PartieDeChasseService.Tirer(id, "Bernard");

                tirerQuandPartieExistePas.Should()
                    .Throw<LaPartieDeChasseNexistePas>();
                Repository.SavedPartieDeChasse().Should().BeNull();
            }

            [Fact]
            public void AvecUnChasseurNayantPlusDeBalles()
            {
                var id = Guid.NewGuid();
                Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20},
                        new("Bernard") {BallesRestantes = 0},
                        new("Robert") {BallesRestantes = 12},
                    }));

                var tirerSansBalle = () => PartieDeChasseService.Tirer(id, "Bernard");

                tirerSansBalle.Should()
                    .Throw<TasPlusDeBallesMonVieuxChasseALaMain>();

                AssertLastEvent(Repository.SavedPartieDeChasse()!,
                    "Bernard tire -> T'as plus de balles mon vieux, chasse à la main");
            }

            [Fact]
            public void CarLeChasseurNestPasDansLaPartie()
            {
                var id = Guid.NewGuid();
                Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20},
                        new("Bernard") {BallesRestantes = 8},
                        new("Robert") {BallesRestantes = 12},
                    }));

                var chasseurInconnuVeutTirer = () => PartieDeChasseService.Tirer(id, "Chasseur inconnu");

                chasseurInconnuVeutTirer
                    .Should()
                    .Throw<ChasseurInconnu>()
                    .WithMessage("Chasseur inconnu Chasseur inconnu");

                Repository.SavedPartieDeChasse().Should().BeNull();
            }

            [Fact]
            public void SiLesChasseursSontEnApero()
            {
                var id = Guid.NewGuid();
                Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20},
                        new("Bernard") {BallesRestantes = 8},
                        new("Robert") {BallesRestantes = 12},
                    }, PartieStatus.Apéro));

                var tirerEnPleinApéro = () => PartieDeChasseService.Tirer(id, "Chasseur inconnu");

                tirerEnPleinApéro.Should()
                    .Throw<OnTirePasPendantLapéroCestSacré>();

                AssertLastEvent(Repository.SavedPartieDeChasse()!,
                    "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");
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

                var tirerQuandTerminée = () => PartieDeChasseService.Tirer(id, "Chasseur inconnu");

                tirerQuandTerminée.Should()
                    .Throw<OnTirePasQuandLaPartieEstTerminée>();

                AssertLastEvent(Repository.SavedPartieDeChasse()!,
                    "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
            }
        }
    }
}