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

            When(id => _useCase.Handle(id));

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

                When(id => _useCase.Handle(id));

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

                When(id => _useCase.Handle(id));

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

                When(id => _useCase.Handle(id));

                ThenThrow<OnPrendPasLapéroQuandLaPartieEstTerminée>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should().BeNull());
            }
        }
    }
}