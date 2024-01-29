using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Builders;

namespace Bouchonnois.Tests.Unit
{
    public class TerminerLaPartieDeChasse : PartieDeChasseServiceTest
    {
        [Fact]
        public void QuandLaPartieEstEnCoursEt1SeulChasseurGagne()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Bernard(), Robert().AyantTué(2))
                ));

            string? winner = null;
            When(id => winner = PartieDeChasseService.TerminerLaPartie(id));

            Then(savedPartieDeChasse =>
                    savedPartieDeChasse.Should()
                        .HaveEmittedEvent(Now, "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes"),
                () => winner.Should().Be(Data.Robert));
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEt1SeulChasseurDansLaPartie()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Robert().AyantTué(2))
                )
            );

            string? winner = null;
            When(id => winner = PartieDeChasseService.TerminerLaPartie(id));

            Then(savedPartieDeChasse =>
                    savedPartieDeChasse.Should()
                        .HaveEmittedEvent(Now, "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes"),
                () => winner.Should().Be(Data.Robert));
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEt2ChasseursExAequo()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé().AyantTué(2), Bernard().AyantTué(2), Robert())
                )
            );

            string? winner = null;
            When(id => winner = PartieDeChasseService.TerminerLaPartie(id));

            Then(savedPartieDeChasse =>
                    savedPartieDeChasse.Should()
                        .HaveEmittedEvent(Now,
                            "La partie de chasse est terminée, vainqueur : Dédé - 2 galinettes, Bernard - 2 galinettes"),
                () => winner.Should().Be("Dédé, Bernard"));
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEtToutLeMondeBrocouille()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                )
            );

            string? winner = null;
            When(id => winner = PartieDeChasseService.TerminerLaPartie(id));

            Then(savedPartieDeChasse =>
                    savedPartieDeChasse.Should()
                        .HaveEmittedEvent(Now, "La partie de chasse est terminée, vainqueur : Brocouille"),
                () => winner.Should().Be("Brocouille"));
        }

        [Fact]
        public void QuandLesChasseursSontALaperoEtTousExAequo()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé().AyantTué(3), Bernard().AyantTué(3), Robert().AyantTué(3))
                        .ALapéro()
                )
            );

            string? winner = null;
            When(id => winner = PartieDeChasseService.TerminerLaPartie(id));

            Then(savedPartieDeChasse =>
                    savedPartieDeChasse.Should()
                        .HaveEmittedEvent(Now,
                            "La partie de chasse est terminée, vainqueur : Dédé - 3 galinettes, Bernard - 3 galinettes, Robert - 3 galinettes"),
                () => winner.Should().Be("Dédé, Bernard, Robert"));
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void SiLaPartieDeChasseEstDéjàTerminée()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Terminée())
                );

                When(id => PartieDeChasseService.TerminerLaPartie(id));

                ThenThrow<QuandCestFiniCestFini>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }
        }
    }
}