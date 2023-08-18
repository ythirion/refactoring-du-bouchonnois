using Bouchonnois.Domain.Exceptions;
using Bouchonnois.UseCases;
using Bouchonnois.UseCases.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class PrendreLApéro : UseCaseTest<PrendreLapéro>
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

            WhenWithException(id => _useCase.Handle(new Domain.Commands.PrendreLapéro(id)));

            Then(savedPartieDeChasse =>
                savedPartieDeChasse.Should()
                    .HaveEmittedEvent(Now, "Petit apéro")
                    .And
                    .BeInApéro());
        }

        public class Echoue : UseCaseTest<PrendreLapéro>
        {
            public Echoue() : base((r, p) => new PrendreLapéro(r, p))
            {
            }

            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                WhenWithException(id => _useCase.Handle(new Domain.Commands.PrendreLapéro(id)));

                ThenThrow<LaPartieDeChasseNexistePas>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }

            [Fact]
            public void SiLesChasseursSontDéjaEnApero()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .ALapéro())
                );

                WhenWithException(id => _useCase.Handle(new Domain.Commands.PrendreLapéro(id)));

                ThenThrow<OnEstDéjàEnTrainDePrendreLapéro>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should().BeNull());
            }

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Terminée())
                );

                WhenWithException(id => _useCase.Handle(new Domain.Commands.PrendreLapéro(id)));

                ThenThrow<OnPrendPasLapéroQuandLaPartieEstTerminée>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should().BeNull());
            }
        }
    }
}