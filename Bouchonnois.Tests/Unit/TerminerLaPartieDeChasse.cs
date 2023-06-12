using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class TerminerLaPartieDeChasse : PartieDeChasseServiceTest
    {
        [Fact]
        public void QuandLaPartieEstEnCoursEt1SeulChasseurGagne()
        {
            var id = Guid.NewGuid();
            Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                new List<Chasseur>
                {
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12, NbGalinettes = 2},
                }));

            var meilleurChasseur = PartieDeChasseService.TerminerLaPartie(id);

            var savedPartieDeChasse = Repository.SavedPartieDeChasse();
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
            AssertLastEvent(Repository.SavedPartieDeChasse()!,
                "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEt1SeulChasseurDansLaPartie()
        {
            var id = Guid.NewGuid();
            Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                new List<Chasseur>
                {
                    new("Robert") {BallesRestantes = 12, NbGalinettes = 2},
                }));

            var meilleurChasseur = PartieDeChasseService.TerminerLaPartie(id);

            var savedPartieDeChasse = Repository.SavedPartieDeChasse();
            savedPartieDeChasse!.Id.Should().Be(id);
            savedPartieDeChasse.Status.Should().Be(PartieStatus.Terminée);
            savedPartieDeChasse.Terrain.Nom.Should().Be("Pitibon sur Sauldre");
            savedPartieDeChasse.Terrain.NbGalinettes.Should().Be(3);
            savedPartieDeChasse.Chasseurs.Should().HaveCount(1);
            savedPartieDeChasse.Chasseurs[0].Nom.Should().Be("Robert");
            savedPartieDeChasse.Chasseurs[0].BallesRestantes.Should().Be(12);
            savedPartieDeChasse.Chasseurs[0].NbGalinettes.Should().Be(2);

            meilleurChasseur.Should().Be("Robert");
            AssertLastEvent(Repository.SavedPartieDeChasse()!,
                "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEt2ChasseursExAequo()
        {
            var id = Guid.NewGuid();
            Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                new List<Chasseur>
                {
                    new("Dédé") {BallesRestantes = 20, NbGalinettes = 2},
                    new("Bernard") {BallesRestantes = 8, NbGalinettes = 2},
                    new("Robert") {BallesRestantes = 12},
                }));

            var meilleurChasseur = PartieDeChasseService.TerminerLaPartie(id);

            var savedPartieDeChasse = Repository.SavedPartieDeChasse();
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
            AssertLastEvent(Repository.SavedPartieDeChasse()!,
                "La partie de chasse est terminée, vainqueur : Dédé - 2 galinettes, Bernard - 2 galinettes");
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEtToutLeMondeBrocouille()
        {
            var id = Guid.NewGuid();
            Repository.Add(new PartieDeChasse(
                id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                new List<Chasseur>
                {
                    new("Dédé") {BallesRestantes = 20},
                    new("Bernard") {BallesRestantes = 8},
                    new("Robert") {BallesRestantes = 12},
                }
                , new List<Event>()
            ));

            var meilleurChasseur = PartieDeChasseService.TerminerLaPartie(id);

            var savedPartieDeChasse = Repository.SavedPartieDeChasse();
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
            AssertLastEvent(Repository.SavedPartieDeChasse()!,
                "La partie de chasse est terminée, vainqueur : Brocouille");
        }

        [Fact]
        public void QuandLesChasseursSontALaperoEtTousExAequo()
        {
            var id = Guid.NewGuid();

            Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                new List<Chasseur>
                {
                    new("Dédé") {BallesRestantes = 20, NbGalinettes = 3},
                    new("Bernard") {BallesRestantes = 8, NbGalinettes = 3},
                    new("Robert") {BallesRestantes = 12, NbGalinettes = 3},
                }, PartieStatus.Apéro));

            var meilleurChasseur = PartieDeChasseService.TerminerLaPartie(id);

            var savedPartieDeChasse = Repository.SavedPartieDeChasse();
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
            AssertLastEvent(Repository.SavedPartieDeChasse()!,
                "La partie de chasse est terminée, vainqueur : Dédé - 3 galinettes, Bernard - 3 galinettes, Robert - 3 galinettes");
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void SiLaPartieDeChasseEstDéjàTerminée()
            {
                var id = Guid.NewGuid();

                Repository.Add(new PartieDeChasse(id, new Terrain("Pitibon sur Sauldre") {NbGalinettes = 3},
                    new List<Chasseur>
                    {
                        new("Dédé") {BallesRestantes = 20},
                        new("Bernard") {BallesRestantes = 8},
                        new("Robert") {BallesRestantes = 12},
                    }, PartieStatus.Terminée));

                var prendreLapéroQuandTerminée = () => PartieDeChasseService.TerminerLaPartie(id);

                prendreLapéroQuandTerminée.Should()
                    .Throw<QuandCestFiniCestFini>();

                Repository.SavedPartieDeChasse().Should().BeNull();
            }
        }
    }
}