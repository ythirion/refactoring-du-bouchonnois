using Bouchonnois.UseCases;

namespace Bouchonnois.Tests.Unit
{
    public class ReprendreLaPartieDeChasse : UseCaseTest<ReprendreLaPartie, VoidResponse>
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

            When(id => _useCase.Handle(new Domain.Commands.ReprendreLaPartie(id)));

            Then((_, savedPartieDeChasse) => savedPartieDeChasse.Should()
                .HaveEmittedEvent(Now, "Reprise de la chasse")
                .And
                .BeEnCours());
        }

        public class Echoue : UseCaseTest<ReprendreLaPartie, VoidResponse>
        {
            public Echoue() : base((r, p) => new ReprendreLaPartie(r, p))
            {
            }

            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                When(id => _useCase.Handle(new Domain.Commands.ReprendreLaPartie(id)));

                ThenFailWith(
                    $"La partie de chasse {_partieDeChasseId} n'existe pas",
                    savedPartieDeChasse => savedPartieDeChasse.Should().BeNull()
                );
            }

            [Fact]
            public void SiLaChasseEstEnCours()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                ));

                When(id => _useCase.Handle(new Domain.Commands.ReprendreLaPartie(id)));

                ThenFailWith($"La partie de chasse est déjà en cours");
            }

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Terminée()
                ));

                When(id => _useCase.Handle(new Domain.Commands.ReprendreLaPartie(id)));

                ThenFailWith("La partie de chasse est déjà terminée");
            }
        }
    }
}