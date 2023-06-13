using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class TirerSurUneGalinette : PartieDeChasseServiceTest
    {
        [Fact]
        public void AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain()
        {
            PartieDeChasseService.TirerSurUneGalinette(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                ).Id, "Bernard");

            SavedPartieDeChasse()
                .Should()
                .HaveEmittedEvent(Now, "Bernard tire sur une galinette").And
                .ChasseurATiréSurUneGalinette("Bernard", 7, 1).And
                .GalinettesSurLeTerrain(2);
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void CarPartieNexistePas()
                => ExecuteAndAssertThrow<LaPartieDeChasseNexistePas>(
                    s => s.TirerSurUneGalinette(UnePartieDeChasseInexistante(), "Bernard"),
                    p => p.Should().BeNull());

            [Fact]
            public void AvecUnChasseurNayantPlusDeBalles()
                => ExecuteAndAssertThrow<TasPlusDeBallesMonVieuxChasseALaMain>(
                    s => s.TirerSurUneGalinette(
                        UnePartieDeChasseExistante(
                            SurUnTerrainRicheEnGalinettes()
                                .Avec(Dédé(), Bernard().SansBalles(), Robert())
                        ).Id, "Bernard"),
                    p => p.Should().HaveEmittedEvent(Now,
                        "Bernard veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main")
                );

            [Fact]
            public void CarPasDeGalinetteSurLeTerrain()
                => ExecuteAndAssertThrow<TasTropPicoléMonVieuxTasRienTouché>(
                    s => s.TirerSurUneGalinette(
                        UnePartieDeChasseExistante(
                            SurUnTerrainSansGalinettes()
                                .Avec(Dédé(), Robert())
                        ).Id, "Bernard"),
                    p => p.Should().BeNull());

            [Fact]
            public void CarLeChasseurNestPasDansLaPartie()
                => ExecuteAndAssertThrow<ChasseurInconnu>(
                        s => s.TirerSurUneGalinette(
                            UnePartieDeChasseExistante(
                                SurUnTerrainRicheEnGalinettes()
                                    .Avec(Dédé(), Robert())
                            ).Id, "Chasseur inconnu"),
                        s => s.Should().BeNull())
                    .WithMessage("Chasseur inconnu Chasseur inconnu");

            [Fact]
            public void SiLesChasseursSontEnApero()
                => ExecuteAndAssertThrow<OnTirePasPendantLapéroCestSacré>(
                    s => s.TirerSurUneGalinette(
                        UnePartieDeChasseExistante(
                            SurUnTerrainRicheEnGalinettes()
                                .Avec(Dédé(), Robert())
                                .ALapéro()
                        ).Id, "Chasseur inconnu"),
                    p => p.Should().HaveEmittedEvent(Now,
                        "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
                => ExecuteAndAssertThrow<OnTirePasQuandLaPartieEstTerminée>(
                    s => s.TirerSurUneGalinette(
                        UnePartieDeChasseExistante(
                            SurUnTerrainRicheEnGalinettes()
                                .Avec(Dédé(), Robert())
                                .Terminée()
                        ).Id, "Chasseur inconnu"),
                    p => p.Should().HaveEmittedEvent(Now,
                        "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée"));
        }
    }
}