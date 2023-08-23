using Bouchonnois.Domain.Apéro;
using Bouchonnois.UseCases;
using PrendreLapéro = Bouchonnois.UseCases.PrendreLapéro;

namespace Bouchonnois.Tests.Unit
{
    public class PrendreLApéro : UseCaseTest<PrendreLapéro, VoidResponse>
    {
        public PrendreLApéro() : base((r, p) => new PrendreLapéro(r, p))
        {
        }

        [Fact]
        public void QuandLaPartieEstEnCours()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                )
            );

            When(id => _useCase.Handle(new Domain.Apéro.PrendreLapéro(id)));

            Then((response, partieDeChasse) =>
            {
                response.Should().Be(VoidResponse.Empty);
                partieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository, new ApéroDémarré(partieDeChasse!.Id, Now));
            });
        }

        public class Echoue : UseCaseTest<PrendreLapéro, VoidResponse>
        {
            public Echoue() : base((r, p) => new PrendreLapéro(r, p))
            {
            }

            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                When(id => _useCase.Handle(new Domain.Apéro.PrendreLapéro(id)));

                ThenFailWith(
                    $"La partie de chasse {_partieDeChasseId} n'existe pas",
                    savedPartieDeChasse => savedPartieDeChasse.Should().BeNull()
                );
            }

            [Fact]
            public void SiLesChasseursSontDéjaEnApero()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .ALapéro())
                );

                When(id => _useCase.Handle(new Domain.Apéro.PrendreLapéro(id)));

                ThenFailWith("On est déjà en plein apéro");
            }

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Terminée())
                );

                When(id => _useCase.Handle(new Domain.Apéro.PrendreLapéro(id)));

                ThenFailWith("La partie de chasse est déjà terminée");
            }
        }
    }
}