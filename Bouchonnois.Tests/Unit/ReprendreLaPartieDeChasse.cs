using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class ReprendreLaPartieDeChasse : PartieDeChasseServiceTest
    {
        [Fact]
        public void QuandLapéroEstEnCours()
        {
            PartieDeChasseService.ReprendreLaPartie(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .ALapéro()
                ).Id);

            SavedPartieDeChasse()
                .Should()
                .HaveEmittedEvent(Now, "Reprise de la chasse")
                .And
                .BeEnCours();
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void CarPartieNexistePas()
                => ExecuteAndAssertThrow<LaPartieDeChasseNexistePas>(
                    s => s.ReprendreLaPartie(UnePartieDeChasseInexistante()),
                    p => p.Should().BeNull());

            [Fact]
            public void SiLaChasseEstEnCours()
                => ExecuteAndAssertThrow<LaChasseEstDéjàEnCours>(
                    s => s.ReprendreLaPartie(
                        UnePartieDeChasseExistante(
                            SurUnTerrainRicheEnGalinettes()
                        ).Id),
                    p => p.Should().BeNull());

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
                => ExecuteAndAssertThrow<QuandCestFiniCestFini>(
                    s => s.ReprendreLaPartie(
                        UnePartieDeChasseExistante(
                            SurUnTerrainRicheEnGalinettes()
                                .Terminée()
                        ).Id),
                    p => p.Should().BeNull());
        }
    }
}