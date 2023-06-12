using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class TirerSurUneGalinette : PartieDeChasseServiceTest
    {
        [Fact]
        public void AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain()
        {
            var partieDeChasse = UnePartieDeChasseExistante(
                UnePartieDeChasseDuBouchonnois()
                    .SurUnTerrainRicheEnGalinettes()
                    .Avec(Dédé(), Bernard(), Robert())
            );

            PartieDeChasseService.TirerSurUneGalinette(partieDeChasse.Id, "Bernard");

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
            {
                var partieDeChasse = UnePartieDeChasseExistante(
                    UnePartieDeChasseDuBouchonnois()
                        .SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Bernard().SansBalles(), Robert())
                );

                ExecuteAndAssertThrow<TasPlusDeBallesMonVieuxChasseALaMain>(
                    s => s.TirerSurUneGalinette(partieDeChasse.Id, "Bernard"),
                    p => p.Should().HaveEmittedEvent(Now,
                        "Bernard veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main")
                );
            }

            [Fact]
            public void CarPasDeGalinetteSurLeTerrain()
            {
                var partieDeChasse = UnePartieDeChasseExistante(
                    UnePartieDeChasseDuBouchonnois()
                        .SurUnTerrainSansGalinettes()
                        .Avec(Dédé(), Robert())
                );

                ExecuteAndAssertThrow<TasTropPicoléMonVieuxTasRienTouché>(
                    s => s.TirerSurUneGalinette(partieDeChasse.Id, "Bernard"),
                    p => p.Should().BeNull());
            }

            [Fact]
            public void CarLeChasseurNestPasDansLaPartie()
            {
                var partieDeChasse = UnePartieDeChasseExistante(
                    UnePartieDeChasseDuBouchonnois()
                        .SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Robert())
                );

                ExecuteAndAssertThrow<ChasseurInconnu>(
                        s => s.TirerSurUneGalinette(partieDeChasse.Id, "Chasseur inconnu"),
                        s => s.Should().BeNull())
                    .WithMessage("Chasseur inconnu Chasseur inconnu");
            }

            [Fact]
            public void SiLesChasseursSontEnApero()
            {
                var partieDeChasse = UnePartieDeChasseExistante(
                    UnePartieDeChasseDuBouchonnois()
                        .SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Robert())
                        .ALapéro()
                );

                ExecuteAndAssertThrow<OnTirePasPendantLapéroCestSacré>(
                    s => s.TirerSurUneGalinette(partieDeChasse.Id, "Chasseur inconnu"),
                    p => p.Should().HaveEmittedEvent(Now,
                        "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));
            }

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
            {
                var partieDeChasse = UnePartieDeChasseExistante(
                    UnePartieDeChasseDuBouchonnois()
                        .SurUnTerrainRicheEnGalinettes()
                        .Avec(Dédé(), Robert())
                        .Terminée()
                );

                ExecuteAndAssertThrow<OnTirePasQuandLaPartieEstTerminée>(
                    s => s.TirerSurUneGalinette(partieDeChasse.Id, "Chasseur inconnu"),
                    p => p.Should().HaveEmittedEvent(Now,
                        "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée"));
            }
        }
    }
}