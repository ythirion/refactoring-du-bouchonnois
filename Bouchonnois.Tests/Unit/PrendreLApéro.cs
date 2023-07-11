using Bouchonnois.Service.Exceptions;
using Bouchonnois.UseCases;

namespace Bouchonnois.Tests.Unit
{
    public class PrendreLApéro : PartieDeChasseServiceTest
    {
        private readonly PrendreLapéro _useCase;

        public PrendreLApéro() => _useCase = new PrendreLapéro(Repository, TimeProvider);

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

        public class Echoue : PrendreLApéro
        {
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