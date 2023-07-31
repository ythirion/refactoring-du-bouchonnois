using Bouchonnois.Domain.Exceptions;
using Bouchonnois.Tests.Builders;
using Bouchonnois.UseCases.Exceptions;

namespace Bouchonnois.Tests.Unit
{
    public class Tirer : UseCaseTest<UseCases.Tirer>
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

            When(id => _useCase.Handle(id, Data.Bernard));

            Then(savedPartieDeChasse =>
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now, "Bernard tire").And
                    .ChasseurATiré(Data.Bernard, ballesRestantes: 7).And
                    .GalinettesSurLeTerrain(3)
            );
        }

        public class Echoue : UseCaseTest<UseCases.Tirer>
        {
            public Echoue() : base((r, p) => new UseCases.Tirer(r, p))
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
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainSansGalinettes()
                            .Avec(Dédé(), Bernard().SansBalles(), Robert())
                    ));

                When(id => _useCase.Handle(id, Data.Bernard));

                ThenThrow<TasPlusDeBallesMonVieuxChasseALaMain>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should()
                        .HaveEmittedEvent(Now, "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"));
            }

            [Fact]
            public void CarLeChasseurNestPasDansLaPartie()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                    ));

                When(id => _useCase.Handle(id, Data.ChasseurInconnu));

                ThenThrow<ChasseurInconnu>(
                    savedPartieDeChasse => savedPartieDeChasse.Should().BeNull(),
                    expectedMessage: "Chasseur inconnu Chasseur inconnu");
            }

            [Fact]
            public void SiLesChasseursSontEnApero()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .ALapéro()
                    ));

                When(id => _useCase.Handle(id, Data.ChasseurInconnu));

                ThenThrow<OnTirePasPendantLapéroCestSacré>(
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

                When(id => _useCase.Handle(id, Data.ChasseurInconnu));

                ThenThrow<OnTirePasQuandLaPartieEstTerminée>(
                    savedPartieDeChasse =>
                        savedPartieDeChasse
                            .Should()
                            .HaveEmittedEvent(Now,
                                "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée"));
            }
        }
    }
}