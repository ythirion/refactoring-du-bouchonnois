using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class ReprendreLaPartieDeChasse : PartieDeChasseServiceTest
    {
        [Fact]
        public void QuandLapéroEstEnCours()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .ALapéro()
                ));

            When(id => PartieDeChasseService.ReprendreLaPartie(id));

            Then(savedPartieDeChasse => savedPartieDeChasse.Should()
                .HaveEmittedEvent(Now, "Reprise de la chasse")
                .And
                .BeEnCours());
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                When(id => PartieDeChasseService.ReprendreLaPartie(id));

                ThenThrow<LaPartieDeChasseNexistePas>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }

            [Fact]
            public void SiLaChasseEstEnCours()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                ));

                When(id => PartieDeChasseService.ReprendreLaPartie(id));

                ThenThrow<LaChasseEstDéjàEnCours>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Terminée()
                ));

                When(id => PartieDeChasseService.ReprendreLaPartie(id));

                ThenThrow<QuandCestFiniCestFini>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }
        }
    }
}