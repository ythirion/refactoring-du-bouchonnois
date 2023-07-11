using Bouchonnois.Service.Exceptions;
using Bouchonnois.UseCases;

namespace Bouchonnois.Tests.Unit
{
    public class ReprendreLaPartieDeChasse : PartieDeChasseServiceTest
    {
        private readonly ReprendreLaPartie _useCase;

        public ReprendreLaPartieDeChasse() => _useCase = new ReprendreLaPartie(Repository, TimeProvider);

        [Fact]
        public void QuandLapéroEstEnCours()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .ALapéro()
                ));

            When(id => _useCase.Handle(id));

            Then(savedPartieDeChasse => savedPartieDeChasse.Should()
                .HaveEmittedEvent(Now, "Reprise de la chasse")
                .And
                .BeEnCours());
        }

        public class Echoue : ReprendreLaPartieDeChasse
        {
            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                When(id => _useCase.Handle(id));

                ThenThrow<LaPartieDeChasseNexistePas>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }

            [Fact]
            public void SiLaChasseEstEnCours()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                ));

                When(id => _useCase.Handle(id));

                ThenThrow<LaChasseEstDéjàEnCours>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Terminée()
                ));

                When(id => _useCase.Handle(id));

                ThenThrow<QuandCestFiniCestFini>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }
        }
    }
}