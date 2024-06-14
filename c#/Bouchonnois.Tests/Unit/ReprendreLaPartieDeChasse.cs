using Bouchonnois.Domain.Reprendre;
using Bouchonnois.UseCases;
using ReprendreLaPartie = Bouchonnois.UseCases.ReprendreLaPartie;

namespace Bouchonnois.Tests.Unit
{
    public class ReprendreLaPartieDeChasse : UseCaseTest<ReprendreLaPartie, VoidResponse>
    {
        public ReprendreLaPartieDeChasse() : base((r, p) => new ReprendreLaPartie(r))
        {
        }

        [Fact]
        public async Task QuandLapéroEstEnCours()
        {
            Given(
                await UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .ALapéro()
                ));

            await When(id => UseCase.Handle(new Domain.Reprendre.ReprendreLaPartie(id)));

            Then((_, partieDeChasse) =>
                partieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository, new PartieReprise(partieDeChasse!.Id, Now)));
        }

        public class Echoue : UseCaseTest<ReprendreLaPartie, VoidResponse>
        {
            public Echoue() : base((r, p) => new ReprendreLaPartie(r))
            {
            }

            [Fact]
            public async Task CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                await When(id => UseCase.Handle(new Domain.Reprendre.ReprendreLaPartie(id)));

                ThenFailWith(
                    $"La partie de chasse {PartieDeChasseId} n'existe pas",
                    partieDeChasse => partieDeChasse.Should().BeNull()
                );
            }

            [Fact]
            public async Task SiLaChasseEstEnCours()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                    ));

                await When(id => UseCase.Handle(new Domain.Reprendre.ReprendreLaPartie(id)));

                ThenFailWith($"La partie de chasse est déjà en cours");
            }

            [Fact]
            public async Task SiLaPartieDeChasseEstTerminée()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Terminée()
                    ));

                await When(id => UseCase.Handle(new Domain.Reprendre.ReprendreLaPartie(id)));

                ThenFailWith("La partie de chasse est déjà terminée");
            }
        }
    }
}