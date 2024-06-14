using Bouchonnois.Service.Exceptions;
using FsCheck;
using FsCheck.Xunit;

namespace Bouchonnois.Tests.Unit
{
    public class PrendreLApéro : PartieDeChasseServiceTest
    {
        [Fact]
        public void QuandLaPartieEstEnCours()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                )
            );

            When(id => PartieDeChasseService.PrendreLapéro(id));

            Then(savedPartieDeChasse =>
                savedPartieDeChasse.Should()
                    .HaveEmittedEvent(Now, "Petit apéro")
                    .And
                    .BeInApéro());
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                When(id => PartieDeChasseService.PrendreLapéro(id));

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

                When(id => PartieDeChasseService.PrendreLapéro(id));

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

                When(id => PartieDeChasseService.PrendreLapéro(id));

                ThenThrow<OnPrendPasLapéroQuandLaPartieEstTerminée>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should().BeNull());
            }
        }
    }
}