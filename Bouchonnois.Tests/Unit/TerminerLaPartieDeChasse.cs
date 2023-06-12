using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class TerminerLaPartieDeChasse : PartieDeChasseServiceTest
    {
        [Fact]
        public void QuandLaPartieEstEnCoursEt1SeulChasseurGagne()
        {
            PartieDeChasseService
                .TerminerLaPartie(
                    UnePartieDeChasseExistante(
                        UnePartieDeChasseDuBouchonnois()
                            .Avec(Dédé(), Bernard(), Robert().AyantTué(2))
                    ).Id)
                .Should()
                .Be("Robert");

            SavedPartieDeChasse()
                .Should()
                .HaveEmittedEvent(Now, "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEt1SeulChasseurDansLaPartie()
        {
            PartieDeChasseService
                .TerminerLaPartie(
                    UnePartieDeChasseExistante(
                        UnePartieDeChasseDuBouchonnois()
                            .Avec(Robert().AyantTué(2))
                    ).Id)
                .Should()
                .Be("Robert");

            SavedPartieDeChasse()
                .Should()
                .HaveEmittedEvent(Now, "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEt2ChasseursExAequo()
        {
            PartieDeChasseService
                .TerminerLaPartie(
                    UnePartieDeChasseExistante(
                        UnePartieDeChasseDuBouchonnois()
                            .Avec(Dédé().AyantTué(2), Bernard().AyantTué(2), Robert())
                    ).Id)
                .Should()
                .Be("Dédé, Bernard");

            SavedPartieDeChasse()
                .Should()
                .HaveEmittedEvent(Now,
                    "La partie de chasse est terminée, vainqueur : Dédé - 2 galinettes, Bernard - 2 galinettes");
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEtToutLeMondeBrocouille()
        {
            PartieDeChasseService
                .TerminerLaPartie(
                    UnePartieDeChasseExistante(UnePartieDeChasseDuBouchonnois()).Id)
                .Should()
                .Be("Brocouille");

            SavedPartieDeChasse()
                .Should()
                .HaveEmittedEvent(Now,
                    "La partie de chasse est terminée, vainqueur : Brocouille");
        }

        [Fact]
        public void QuandLesChasseursSontALaperoEtTousExAequo()
        {
            PartieDeChasseService
                .TerminerLaPartie(
                    UnePartieDeChasseExistante(
                        UnePartieDeChasseDuBouchonnois()
                            .Avec(Dédé().AyantTué(3), Bernard().AyantTué(3), Robert().AyantTué(3))
                            .ALapéro()
                    ).Id)
                .Should()
                .Be("Dédé, Bernard, Robert");

            SavedPartieDeChasse()
                .Should()
                .HaveEmittedEvent(Now,
                    "La partie de chasse est terminée, vainqueur : Dédé - 3 galinettes, Bernard - 3 galinettes, Robert - 3 galinettes");
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void SiLaPartieDeChasseEstDéjàTerminée()
            {
                ExecuteAndAssertThrow<QuandCestFiniCestFini>(
                    s => s.TerminerLaPartie(
                        UnePartieDeChasseExistante(
                            UnePartieDeChasseDuBouchonnois()
                                .Terminée()
                        ).Id),
                    p => p.Should().BeNull());
            }
        }
    }
}