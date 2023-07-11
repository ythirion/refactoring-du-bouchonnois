using Bouchonnois.Domain;
using Bouchonnois.UseCases.Exceptions;
using FsCheck;
using FsCheck.Xunit;
using static FsCheck.Arb;
using static FsCheck.Prop;

namespace Bouchonnois.Tests.Unit
{
    [UsesVerify]
    public class ConsulterStatus : UseCaseTest<UseCases.ConsulterStatus>
    {
        public ConsulterStatus() : base((r, t) => new UseCases.ConsulterStatus(r))
        {
        }

        [Fact]
        public Task QuandLaPartieVientDeDémarrer()
        {
            var id = UnePartieDeChasseExistante(
                SurUnTerrainRicheEnGalinettes()
                    .Avec(Dédé(), Bernard(), Robert().AyantTué(2))
                    .Events(new Event(new DateTime(2024, 4, 25, 9, 0, 12),
                        "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"))
            ).Id;

            return Verify(_useCase.Handle(id))
                .DontScrubDateTimes();
        }

        [Fact]
        public Task QuandLaPartieEstTerminée()
        {
            var id = UnePartieDeChasseExistante(
                SurUnTerrainRicheEnGalinettes()
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
                    )).Id;

            return Verify(_useCase.Handle(id))
                .DontScrubDateTimes();
        }

        public class Echoue : UseCaseTest<UseCases.ConsulterStatus>
        {
            private readonly Arbitrary<Guid> _nonExistingPartiesDeChasse = Generate<Guid>().ToArbitrary();

            public Echoue() : base((r, t) => new UseCases.ConsulterStatus(r))

            {
            }

            [Property]
            public Property CarPartieNexistePas()
                => ForAll(
                    _nonExistingPartiesDeChasse,
                    id => MustFailWith<LaPartieDeChasseNexistePas>(
                        () => _useCase.Handle(id),
                        savedPartieDeChasse => savedPartieDeChasse == null
                    )
                );
        }
    }
}