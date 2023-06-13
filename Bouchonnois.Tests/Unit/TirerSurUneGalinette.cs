using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class TirerSurUneGalinette : PartieDeChasseServiceTest
    {
        [Fact]
        public void AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain()
        {
            Given(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                ));

            When(id => PartieDeChasseService.TirerSurUneGalinette(id, Bernard));

            Then(savedPartieDeChasse =>
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now, "Bernard tire sur une galinette").And
                    .ChasseurATiréSurUneGalinette(Bernard, ballesRestantes: 7, galinettes: 1).And
                    .GalinettesSurLeTerrain(2)
            );
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                When(id => PartieDeChasseService.TirerSurUneGalinette(id, Bernard));

                ThenThrow<LaPartieDeChasseNexistePas>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }

            [Fact]
            public void AvecUnChasseurNayantPlusDeBalles()
            {
                Given(UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Bernard().SansBalles(), Robert())
                ));

                When(id => PartieDeChasseService.TirerSurUneGalinette(id, Bernard));

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

                When(id => PartieDeChasseService.TirerSurUneGalinette(id, Bernard));

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

                When(id => PartieDeChasseService.TirerSurUneGalinette(id, ChasseurInconnu));

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

                When(id => PartieDeChasseService.TirerSurUneGalinette(id, ChasseurInconnu));

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

                When(id => PartieDeChasseService.TirerSurUneGalinette(id, ChasseurInconnu));

                ThenThrow<OnTirePasQuandLaPartieEstTerminée>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should().HaveEmittedEvent(Now,
                        "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée")
                );
            }
        }
    }
}