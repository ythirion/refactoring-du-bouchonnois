using Bouchonnois.Tests.Builders;
using Bouchonnois.UseCases;

namespace Bouchonnois.Tests.Unit
{
    public class Tirer : UseCaseTestWithoutException<UseCases.Tirer, VoidResponse>
    {
        public Tirer() : base((r, p) => new UseCases.Tirer(r, p))
        {
        }

        [Fact]
        public void AvecUnChasseurAyantDesBalles()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Bernard())
                ));

            When(id => _useCase.HandleSansException(new Domain.Commands.Tirer(id, Data.Bernard)));


            Then((response, savedPartieDeChasse) =>
            {
                response.Should().Be(VoidResponse.Empty);
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now, "Bernard tire").And
                    .ChasseurATiré(Data.Bernard, ballesRestantes: 7).And
                    .GalinettesSurLeTerrain(3);
            });
        }

        public class Echoue : UseCaseTestWithoutException<UseCases.Tirer, VoidResponse>
        {
            public Echoue() : base((r, p) => new UseCases.Tirer(r, p))
            {
            }

            [Fact]
            public void CarPartieNexistePasSansException()
            {
                Given(UnePartieDeChasseInexistante());

                When(partieDeChasseId =>
                    _useCase.HandleSansException(new Domain.Commands.Tirer(partieDeChasseId, Data.Bernard)));

                ThenFailWith(
                    $"La partie de chasse {_partieDeChasseId} n'existe pas",
                    savedPartieDeChasse => savedPartieDeChasse.Should().BeNull()
                );
            }

            [Fact]
            public void CarLeChasseurNestPasDansLaPartie()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                    ));

                When(id => _useCase.HandleSansException(new Domain.Commands.Tirer(id, Data.ChasseurInconnu)));

                ThenFailWith("Chasseur inconnu Chasseur inconnu",
                    savedPartieDeChasse => savedPartieDeChasse.Should().HaveEmittedEvent(Now,
                        "Chasseur inconnu Chasseur inconnu")
                );
            }

            [Fact]
            public void AvecUnChasseurNayantPlusDeBalles()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Avec(Dédé(), Bernard().SansBalles(), Robert())
                    ));

                When(id => _useCase.HandleSansException(new Domain.Commands.Tirer(id, Data.Bernard)));

                ThenFailWith("Bernard tire -> T'as plus de balles mon vieux, chasse à la main",
                    savedPartieDeChasse => savedPartieDeChasse.Should().HaveEmittedEvent(Now,
                        "Bernard tire -> T'as plus de balles mon vieux, chasse à la main")
                );
            }

            [Fact]
            public void SiLesChasseursSontEnApero()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .ALapéro()
                    ));

                When(id => _useCase.HandleSansException(new Domain.Commands.Tirer(id, Data.ChasseurInconnu)));

                ThenFailWith(
                    "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!",
                    savedPartieDeChasse =>
                        savedPartieDeChasse
                            .Should()
                            .HaveEmittedEvent(Now,
                                "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));
            }

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Terminée()
                    ));

                When(id => _useCase.HandleSansException(new Domain.Commands.Tirer(id, Data.ChasseurInconnu)));

                ThenFailWith("Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée",
                    savedPartieDeChasse =>
                        savedPartieDeChasse
                            .Should()
                            .HaveEmittedEvent(Now,
                                "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée"));
            }
        }
    }
}