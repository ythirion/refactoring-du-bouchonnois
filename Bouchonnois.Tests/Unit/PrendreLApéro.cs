using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class PrendreLApéro : PartieDeChasseServiceTest
    {
        [Fact]
        public void QuandLaPartieEstEnCours()
        {
            PartieDeChasseService.PrendreLapéro(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                ).Id);

            SavedPartieDeChasse()
                .Should()
                .HaveEmittedEvent(Now, "Petit apéro")
                .And
                .BeInApéro();
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void CarPartieNexistePas()
                => ExecuteAndAssertThrow<LaPartieDeChasseNexistePas>(
                    s => s.PrendreLapéro(UnePartieDeChasseInexistante()),
                    p => p.Should().BeNull());

            [Fact]
            public void SiLesChasseursSontDéjaEnApero()
                => ExecuteAndAssertThrow<OnEstDéjàEnTrainDePrendreLapéro>(
                    s => s.PrendreLapéro(
                        UnePartieDeChasseExistante(
                            SurUnTerrainRicheEnGalinettes()
                                .ALapéro()
                        ).Id),
                    p => p.Should().BeNull());

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
                => ExecuteAndAssertThrow<OnPrendPasLapéroQuandLaPartieEstTerminée>(
                    s => s.PrendreLapéro(
                        UnePartieDeChasseExistante(
                            SurUnTerrainRicheEnGalinettes()
                                .Terminée()
                        ).Id),
                    p => p.Should().BeNull());
        }
    }
}