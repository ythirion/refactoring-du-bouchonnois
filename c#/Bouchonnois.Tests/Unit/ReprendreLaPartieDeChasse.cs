using Bouchonnois.UseCases;
using Bouchonnois.UseCases.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class ReprendreLaPartieDeChasse : UseCaseTest<ReprendreLaPartie>
    {
        public ReprendreLaPartieDeChasse() : base((r, p) => new ReprendreLaPartie(r, p))
        {
        }

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

        public class Echoue : UseCaseTest<ReprendreLaPartie>
        {
            public Echoue() : base((r, p) => new ReprendreLaPartie(r, p))
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