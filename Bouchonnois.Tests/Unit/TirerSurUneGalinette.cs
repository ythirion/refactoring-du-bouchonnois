using Bouchonnois.Tests.Builders;
using Bouchonnois.UseCases;

namespace Bouchonnois.Tests.Unit
{
    public class TirerSurUneGalinette : UseCaseTest<UseCases.TirerSurUneGalinette, VoidResponse>
    {
        public TirerSurUneGalinette() : base((r, p) => new UseCases.TirerSurUneGalinette(r, p))
        {
        }

        [Fact]
        public void AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                ));

            When(id => _useCase.Handle(new Domain.Commands.TirerSurUneGalinette(id, Data.Bernard)));

            Then((response, savedPartieDeChasse) =>
            {
                response.Should().Be(VoidResponse.Empty);
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now, "Bernard tire sur une galinette").And
                    .ChasseurATiréSurUneGalinette(Data.Bernard, ballesRestantes: 7, galinettes: 1).And
                    .GalinettesSurLeTerrain(2);
            });
        }

        public class Echoue : UseCaseTest<UseCases.TirerSurUneGalinette, VoidResponse>
        {
            public Echoue() : base((r, p) => new UseCases.TirerSurUneGalinette(r, p))
            {
            }

            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                When(id => _useCase.Handle(new Domain.Commands.TirerSurUneGalinette(id, Data.Bernard)));

                ThenFailWith(
                    $"La partie de chasse {_partieDeChasseId} n'existe pas",
                    savedPartieDeChasse => savedPartieDeChasse.Should().BeNull()
                );
            }

            [Fact]
            public void AvecUnChasseurNayantPlusDeBalles()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Bernard().SansBalles(), Robert())
                ));

                When(id => _useCase.Handle(new Domain.Commands.TirerSurUneGalinette(id, Data.Bernard)));

                ThenFailWith("Bernard veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main",
                    savedPartieDeChasse => savedPartieDeChasse.Should().HaveEmittedEvent(Now,
                        "Bernard veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main")
                );
            }

            [Fact]
            public void CarPasDeGalinetteSurLeTerrain()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes(1)
                        .Avec(Dédé().AyantTué(1), Robert())
                ));

                When(id => _useCase.Handle(new Domain.Commands.TirerSurUneGalinette(id, Data.Bernard)));

                ThenFailWith("T'as trop picolé mon vieux, t'as rien touché",
                    savedPartieDeChasse => savedPartieDeChasse.Should().HaveEmittedEvent(Now,
                        "T'as trop picolé mon vieux, t'as rien touché")
                );
            }

            [Fact]
            public void CarLeChasseurNestPasDansLaPartie()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Robert())
                ));

                When(id => _useCase.Handle(new Domain.Commands.TirerSurUneGalinette(id, Data.ChasseurInconnu)));

                ThenFailWith("Chasseur inconnu Chasseur inconnu",
                    savedPartieDeChasse => savedPartieDeChasse.Should().HaveEmittedEvent(Now,
                        "Chasseur inconnu Chasseur inconnu")
                );
            }

            [Fact]
            public void SiLesChasseursSontEnApero()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Robert())
                        .ALapéro()
                ));

                When(id =>
                    _useCase.Handle(new Domain.Commands.TirerSurUneGalinette(id, Data.ChasseurInconnu)));

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
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Robert())
                        .Terminée()
                ));

                When(id => _useCase.Handle(new Domain.Commands.TirerSurUneGalinette(id, Data.ChasseurInconnu)));

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