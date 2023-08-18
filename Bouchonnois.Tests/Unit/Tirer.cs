using Bouchonnois.Domain.Exceptions;
using Bouchonnois.Tests.Builders;
using Bouchonnois.UseCases;
using Bouchonnois.UseCases.Exceptions;

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

            WhenWithException(id => _useCase.Handle(new Domain.Commands.Tirer(id, Data.Bernard)));

            Then(savedPartieDeChasse =>
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Now, "Bernard tire").And
                    .ChasseurATiré(Data.Bernard, ballesRestantes: 7).And
                    .GalinettesSurLeTerrain(3)
            );
        }

        public class Echoue : UseCaseTestWithoutException<UseCases.Tirer, VoidResponse>
        {
            public Echoue() : base((r, p) => new UseCases.Tirer(r, p))
            {
            }

            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                WhenWithException(id => _useCase.Handle(new Domain.Commands.Tirer(id, Data.Bernard)));

                ThenThrow<LaPartieDeChasseNexistePas>(savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }

            [Fact]
            public void CarPartieNexistePasSansException()
            {
                Given(UnePartieDeChasseInexistante());

                When(partieDeChasseId =>
                    _useCase.HandleSansException(new Domain.Commands.Tirer(partieDeChasseId, Data.Bernard),
                        TimeProvider));

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

                When(id => _useCase.HandleSansException(new Domain.Commands.Tirer(id, Data.ChasseurInconnu),
                    TimeProvider));

                ThenFailWith("Chasseur inconnu Chasseur inconnu",
                    savedPartieDeChasse => savedPartieDeChasse.Should().BeNull());
            }

            [Fact]
            public void AvecUnChasseurNayantPlusDeBalles()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Avec(Dédé(), Bernard().SansBalles(), Robert())
                    ));

                WhenWithException(id => _useCase.Handle(new Domain.Commands.Tirer(id, Data.Bernard)));

                ThenThrow<TasPlusDeBallesMonVieuxChasseALaMain>(savedPartieDeChasse =>
                    savedPartieDeChasse.Should()
                        .HaveEmittedEvent(Now, "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"));
            }

            [Fact]
            public void SiLesChasseursSontEnApero()
            {
                Given(
                    UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .ALapéro()
                    ));

                WhenWithException(id => _useCase.Handle(new Domain.Commands.Tirer(id, Data.ChasseurInconnu)));

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

                WhenWithException(id => _useCase.Handle(new Domain.Commands.Tirer(id, Data.ChasseurInconnu)));

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