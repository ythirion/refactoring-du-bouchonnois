using Bouchonnois.Domain.Apéro;
using Bouchonnois.UseCases;
using PrendreLapéro = Bouchonnois.UseCases.PrendreLapéro;

namespace Bouchonnois.Tests.Unit
{
    public class PrendreLApéro : UseCaseTest<PrendreLapéro, VoidResponse>
    {
        public PrendreLApéro() : base((r, p) => new PrendreLapéro(r))
        {
        }

        [Fact]
        public async Task QuandLaPartieEstEnCours()
        {
            Given(
                await UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                )
            );

            await When(id => UseCase.Handle(new Domain.Apéro.PrendreLapéro(id)));

            await Then((response, partieDeChasse) =>
            {
                response.Should().Be(VoidResponse.Empty);
                partieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository, new ApéroDémarré(partieDeChasse!.Id, Now));
            });
        }

        public class Echoue : UseCaseTest<PrendreLapéro, VoidResponse>
        {
            public Echoue() : base((r, p) => new PrendreLapéro(r))
            {
            }

            [Fact]
            public async Task CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                await When(id => UseCase.Handle(new Domain.Apéro.PrendreLapéro(id)));

                await ThenFailWith(
                    $"La partie de chasse {PartieDeChasseId} n'existe pas",
                    partieDeChasse => partieDeChasse.Should().BeNull()
                );
            }

            [Fact]
            public async Task SiLesChasseursSontDéjaEnApero()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .ALapéro())
                );

                await When(id => UseCase.Handle(new Domain.Apéro.PrendreLapéro(id)));

                await ThenFailWith("On est déjà en plein apéro");
            }

            [Fact]
            public async Task SiLaPartieDeChasseEstTerminée()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Terminée())
                );

                await When(id => UseCase.Handle(new Domain.Apéro.PrendreLapéro(id)));

                await ThenFailWith("La partie de chasse est déjà terminée");
            }
        }
    }
}