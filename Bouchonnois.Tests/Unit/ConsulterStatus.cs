using Bouchonnois.Domain;
using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class ConsulterStatus : PartieDeChasseServiceTest
    {
        [Fact]
        public void QuandLaPartieVientDeDémarrer()
        {
            var partieDeChasse = UnePartieDeChasseExistante(
                UnePartieDeChasseDuBouchonnois()
                    .SurUnTerrainRicheEnGalinettes()
                    .Avec(Dédé(), Bernard(), Robert().AyantTué(2))
                    .Events(new Event(new DateTime(2024, 4, 25, 9, 0, 12),
                        "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"))
            );

            PartieDeChasseService.ConsulterStatus(partieDeChasse.Id)
                .Should()
                .Be(
                    "09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
                );
        }

        [Fact]
        public void QuandLaPartieEstTerminée()
        {
            var partieDeChasse = UnePartieDeChasseExistante(
                UnePartieDeChasseDuBouchonnois()
                    .SurUnTerrainRicheEnGalinettes()
                    .Avec(Dédé(), Bernard(), Robert().AyantTué(2))
                    .Events(
                        new Event(new DateTime(2024, 4, 25, 9, 0, 12),
                            "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"),
                        new Event(new DateTime(2024, 4, 25, 9, 10, 0), "Dédé tire"),
                        new Event(new DateTime(2024, 4, 25, 9, 40, 0), "Robert tire sur une galinette"),
                        new Event(new DateTime(2024, 4, 25, 10, 0, 0), "Petit apéro"),
                        new Event(new DateTime(2024, 4, 25, 11, 0, 0), "Reprise de la chasse"),
                        new Event(new DateTime(2024, 4, 25, 11, 2, 0), "Bernard tire"),
                        new Event(new DateTime(2024, 4, 25, 11, 3, 0), "Bernard tire"),
                        new Event(new DateTime(2024, 4, 25, 11, 4, 0), "Dédé tire sur une galinette"),
                        new Event(new DateTime(2024, 4, 25, 11, 30, 0), "Robert tire sur une galinette"),
                        new Event(new DateTime(2024, 4, 25, 11, 40, 0), "Petit apéro"),
                        new Event(new DateTime(2024, 4, 25, 14, 30, 0), "Reprise de la chasse"),
                        new Event(new DateTime(2024, 4, 25, 14, 41, 0), "Bernard tire"),
                        new Event(new DateTime(2024, 4, 25, 14, 41, 1), "Bernard tire"),
                        new Event(new DateTime(2024, 4, 25, 14, 41, 2), "Bernard tire"),
                        new Event(new DateTime(2024, 4, 25, 14, 41, 3), "Bernard tire"),
                        new Event(new DateTime(2024, 4, 25, 14, 41, 4), "Bernard tire"),
                        new Event(new DateTime(2024, 4, 25, 14, 41, 5), "Bernard tire"),
                        new Event(new DateTime(2024, 4, 25, 14, 41, 6), "Bernard tire"),
                        new Event(new DateTime(2024, 4, 25, 14, 41, 7),
                            "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"),
                        new Event(new DateTime(2024, 4, 25, 15, 0, 0), "Robert tire sur une galinette"),
                        new Event(new DateTime(2024, 4, 25, 15, 30, 0),
                            "La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes")
                    )
            );

            PartieDeChasseService
                .ConsulterStatus(partieDeChasse.Id)
                .Should()
                .BeEquivalentTo(
                    @"15:30 - La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes
15:00 - Robert tire sur une galinette
14:41 - Bernard tire -> T'as plus de balles mon vieux, chasse à la main
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:30 - Reprise de la chasse
11:40 - Petit apéro
11:30 - Robert tire sur une galinette
11:04 - Dédé tire sur une galinette
11:03 - Bernard tire
11:02 - Bernard tire
11:00 - Reprise de la chasse
10:00 - Petit apéro
09:40 - Robert tire sur une galinette
09:10 - Dédé tire
09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"
                );
        }

        public class Echoue : PartieDeChasseServiceTest
        {
            [Fact]
            public void CarPartieNexistePas()
                => ExecuteAndAssertThrow<LaPartieDeChasseNexistePas>(
                    s => s.ConsulterStatus(UnePartieDeChasseInexistante()),
                    p => p.Should().BeNull());
        }
    }
}