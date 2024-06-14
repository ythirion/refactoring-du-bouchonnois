using Bouchonnois.Tests.Builders;
using Bouchonnois.UseCases.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class TirerSurUneGalinette : UseCaseTest<UseCases.TirerSurUneGalinette>
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

            When(id => _useCase.Handle(id, Data.Bernard));

            Then(savedPartieDeChasse =>
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now, "Bernard tire sur une galinette").And
                    .ChasseurATiréSurUneGalinette(Data.Bernard, ballesRestantes: 7, galinettes: 1).And
                    .GalinettesSurLeTerrain(2)
            );
        }

        public class Echoue : UseCaseTest<UseCases.TirerSurUneGalinette>
        {
            public Echoue() : base((r, p) => new UseCases.TirerSurUneGalinette(r, p))
            {
            }

            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                When(id => _useCase.Handle(id, Data.Bernard));

                ThenThrow<LaPartieDeChasseNexistePas>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }

            [Fact]
            public void AvecUnChasseurNayantPlusDeBalles()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Bernard().SansBalles(), Robert())
                ));

                When(id => _useCase.Handle(id, Data.Bernard));

                ThenThrow<TasPlusDeBallesMonVieuxChasseALaMain>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should()
                        .HaveEmittedEvent(Now,
                            "Bernard veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main")
                );
            }

            [Fact]
            public void CarPasDeGalinetteSurLeTerrain()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainSansGalinettes()
                        .Avec(Dédé(), Robert())
                ));

                When(id => _useCase.Handle(id, Data.Bernard));

                ThenThrow<TasTropPicoléMonVieuxTasRienTouché>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should().BeNull()
                );
            }

            [Fact]
            public void CarLeChasseurNestPasDansLaPartie()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Robert())
                ));

                When(id => _useCase.Handle(id, Data.ChasseurInconnu));

                ThenThrow<ChasseurInconnu>(savedPartieDeChasse =>
                        savedPartieDeChasse.Should().BeNull(),
                    expectedMessage: "Chasseur inconnu Chasseur inconnu"
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

                When(id => _useCase.Handle(id, Data.ChasseurInconnu));

                ThenThrow<OnTirePasPendantLapéroCestSacré>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should()
                        .HaveEmittedEvent(Now,
                            "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!")
                );
            }

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Robert())
                        .Terminée()
                ));

                When(id => _useCase.Handle(id, Data.ChasseurInconnu));

                ThenThrow<OnTirePasQuandLaPartieEstTerminée>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should().HaveEmittedEvent(Now,
                        "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée")
                );
            }
        }
    }
}