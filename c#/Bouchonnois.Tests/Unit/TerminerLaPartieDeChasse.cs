using Bouchonnois.Tests.Builders;
using Bouchonnois.UseCases;

namespace Bouchonnois.Tests.Unit
{
    public class TerminerLaPartieDeChasse : UseCaseTest<TerminerLaPartie, string>
    {
        public TerminerLaPartieDeChasse() : base((r, p) => new TerminerLaPartie(r, p))
        {
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEt1SeulChasseurGagne()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Bernard(), Robert().AyantTué(2))
                ));

            When(id => _useCase.Handle(new Domain.Commands.TerminerLaPartie(id)));

            Then((winner, savedPartieDeChasse) =>
            {
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now, "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");

                winner.Should().Be(Data.Robert);
            });
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

            When(id => _useCase.Handle(new Domain.Commands.TerminerLaPartie(id)));

            Then((winner, savedPartieDeChasse) =>
            {
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now, "La partie de chasse est terminée, vainqueur : Robert - 2 galinettes");

                winner.Should().Be(Data.Robert);
            });
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEt2ChasseursExAequo()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes(4)
                        .Avec(Dédé().AyantTué(2), Bernard().AyantTué(2), Robert())
                )
            );

            When(id => _useCase.Handle(new Domain.Commands.TerminerLaPartie(id)));

            Then((winner, savedPartieDeChasse) =>
            {
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now,
                        "La partie de chasse est terminée, vainqueur : Dédé - 2 galinettes, Bernard - 2 galinettes");

                winner.Should().Be("Dédé, Bernard");
            });
        }

        [Fact]
        public void QuandLaPartieEstEnCoursEtToutLeMondeBrocouille()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                )
            );

            When(id => _useCase.Handle(new Domain.Commands.TerminerLaPartie(id)));

            Then((winner, savedPartieDeChasse) =>
            {
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now,
                        "La partie de chasse est terminée, vainqueur : Brocouille");

                winner.Should().Be("Brocouille");
            });
        }

        [Fact]
        public void QuandLesChasseursSontALaperoEtTousExAequo()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes(12)
                        .Avec(Dédé().AyantTué(3), Bernard().AyantTué(3), Robert().AyantTué(3))
                        .ALapéro()
                )
            );

            When(id => _useCase.Handle(new Domain.Commands.TerminerLaPartie(id)));

            Then((winner, savedPartieDeChasse) =>
            {
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now,
                        "La partie de chasse est terminée, vainqueur : Dédé - 3 galinettes, Bernard - 3 galinettes, Robert - 3 galinettes");

                winner.Should().Be("Dédé, Bernard, Robert");
            });
        }

        public class Echoue : UseCaseTest<TerminerLaPartie, string>
        {
            public Echoue() : base((r, p) => new TerminerLaPartie(r, p))
            {
            }

            [Fact]
            public void SiLaPartieDeChasseEstDéjàTerminée()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Terminée())
                );

                When(id => _useCase.Handle(new Domain.Commands.TerminerLaPartie(id)));

                ThenFailWith("Quand c'est fini, c'est fini");
            }
        }
    }
}