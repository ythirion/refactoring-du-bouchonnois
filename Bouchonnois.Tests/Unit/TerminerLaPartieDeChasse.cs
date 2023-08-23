using Bouchonnois.Domain.Terminer;
using Bouchonnois.Tests.Builders;
using TerminerLaPartie = Bouchonnois.UseCases.TerminerLaPartie;

namespace Bouchonnois.Tests.Unit
{
    public class TerminerLaPartieDeChasse : UseCaseTest<TerminerLaPartie, string>
    {
        public TerminerLaPartieDeChasse() : base((r, p) => new TerminerLaPartie(r, TimeProvider))
        {
        }

        [Fact]
        public async Task QuandLaPartieEstEnCoursEt1SeulChasseurGagne()
        {
            Given(
                await UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Bernard(), Robert().AyantTué(2))
                ));

            When(id => UseCase.Handle(new Domain.Terminer.TerminerLaPartie(id)));

            Then((winner, savedPartieDeChasse) =>
            {
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository, new PartieTerminée(savedPartieDeChasse!.Id, Now, Data.Robert, 2));

                winner.Should().Be(Data.Robert);
            });
        }

        [Fact]
        public async Task QuandLaPartieEstEnCoursEt1SeulChasseurDansLaPartie()
        {
            Given(
                await UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Robert().AyantTué(2))
                )
            );

            When(id => UseCase.Handle(new Domain.Terminer.TerminerLaPartie(id)));

            Then((winner, savedPartieDeChasse) =>
            {
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository, new PartieTerminée(savedPartieDeChasse!.Id, Now, Data.Robert, 2));

                winner.Should().Be(Data.Robert);
            });
        }

        [Fact]
        public async Task QuandLaPartieEstEnCoursEt2ChasseursExAequo()
        {
            Given(
                await UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes(4)
                        .Avec(Dédé().AyantTué(2), Bernard().AyantTué(2), Robert())
                )
            );

            When(id => UseCase.Handle(new Domain.Terminer.TerminerLaPartie(id)));

            Then((winner, savedPartieDeChasse) =>
            {
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository,
                        new PartieTerminée(savedPartieDeChasse!.Id, Now, 2, Data.Dédé, Data.Bernard));

                winner.Should().Be("Dédé, Bernard");
            });
        }

        [Fact]
        public async Task QuandLaPartieEstEnCoursEtToutLeMondeBrocouille()
        {
            Given(
                await UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                )
            );

            When(id => UseCase.Handle(new Domain.Terminer.TerminerLaPartie(id)));

            Then((winner, savedPartieDeChasse) =>
            {
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository, new PartieTerminée(savedPartieDeChasse!.Id, Now, "Brocouille", 0));

                winner.Should().Be("Brocouille");
            });
        }

        [Fact]
        public async Task QuandLesChasseursSontALaperoEtTousExAequo()
        {
            Given(
                await UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes(12)
                        .Avec(Dédé().AyantTué(3), Bernard().AyantTué(3), Robert().AyantTué(3))
                        .ALapéro()
                )
            );

            When(id => UseCase.Handle(new Domain.Terminer.TerminerLaPartie(id)));

            Then((winner, savedPartieDeChasse) =>
            {
                var partieExAequoTerminée =
                    new PartieTerminée(savedPartieDeChasse!.Id, Now, 3, Data.Dédé, Data.Bernard, Data.Robert);
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository, partieExAequoTerminée);

                partieExAequoTerminée.ToString().Should()
                    .Be("La partie de chasse est terminée, vainqueur : Dédé, Bernard, Robert - 3 galinettes");
                winner.Should().Be("Dédé, Bernard, Robert");
            });
        }

        public class Echoue : UseCaseTest<TerminerLaPartie, string>
        {
            public Echoue() : base((r, p) => new TerminerLaPartie(r, TimeProvider))
            {
            }

            [Fact]
            public async Task SiLaPartieDeChasseEstDéjàTerminée()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Terminée())
                );

                When(id => UseCase.Handle(new Domain.Terminer.TerminerLaPartie(id)));

                ThenFailWith("Quand c'est fini, c'est fini");
            }
        }
    }
}