using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class Tirer : PartieDeChasseServiceTest
    {
        [Fact]
        public void AvecUnChasseurAyantDesBalles()
        {
            PartieDeChasseService.Tirer(
                UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Bernard())
                ).Id, "Bernard");

            SavedPartieDeChasse()
                .Should()
                .HaveEmittedEvent(Now, "Bernard tire").And
                .ChasseurATiré("Bernard", 7).And
                .GalinettesSurLeTerrain(3);
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void CarPartieNexistePas()
                => ExecuteAndAssertThrow<LaPartieDeChasseNexistePas>(
                    s => s.Tirer(UnePartieDeChasseInexistante(), "Bernard"),
                    p => p.Should().BeNull());

            [Fact]
            public void AvecUnChasseurNayantPlusDeBalles()
                => ExecuteAndAssertThrow<TasPlusDeBallesMonVieuxChasseALaMain>(
                    s => s.Tirer(
                        UnePartieDeChasseExistante(
                            SurUnTerrainSansGalinettes()
                                .Avec(Dédé(), Bernard().SansBalles(), Robert())
                        ).Id, "Bernard"),
                    p => p
                        .Should()
                        .HaveEmittedEvent(Now, "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"));

            [Fact]
            public void CarLeChasseurNestPasDansLaPartie()
                => ExecuteAndAssertThrow<ChasseurInconnu>(
                        s => s.Tirer(
                            UnePartieDeChasseExistante(
                                SurUnTerrainRicheEnGalinettes()
                            ).Id, "Chasseur inconnu"),
                        p => p.Should().BeNull())
                    .WithMessage("Chasseur inconnu Chasseur inconnu");

            [Fact]
            public void SiLesChasseursSontEnApero()
                => ExecuteAndAssertThrow<OnTirePasPendantLapéroCestSacré>(
                    s => s.Tirer(
                        UnePartieDeChasseExistante(
                            SurUnTerrainRicheEnGalinettes()
                                .ALapéro()
                        ).Id, "Chasseur inconnu"),
                    p => p
                        .Should()
                        .HaveEmittedEvent(Now,
                            "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));

            [Fact]
            public void SiLaPartieDeChasseEstTerminée()
                => ExecuteAndAssertThrow<OnTirePasQuandLaPartieEstTerminée>(
                    s => s.Tirer(
                        UnePartieDeChasseExistante(
                            SurUnTerrainRicheEnGalinettes()
                                .Terminée()
                        ).Id, "Chasseur inconnu"),
                    p => p
                        .Should()
                        .HaveEmittedEvent(Now,
                            "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée"));
        }
    }
}