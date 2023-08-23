using Bouchonnois.Domain.Tirer;
using Bouchonnois.Tests.Builders;
using Bouchonnois.UseCases;

namespace Bouchonnois.Tests.Unit
{
    public class Tirer : UseCaseTest<UseCases.Tirer, VoidResponse>
    {
        public Tirer() : base((r, p) => new UseCases.Tirer(r, TimeProvider))
        {
        }

        [Fact]
        public async Task AvecUnChasseurAyantDesBalles()
        {
            Given(
                await UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                        .Avec(Bernard())
                ));

            When(id => UseCase.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));


            Then((response, savedPartieDeChasse) =>
            {
                response.Should().Be(VoidResponse.Empty);
                savedPartieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository, new ChasseurATiré(savedPartieDeChasse!.Id, Now, Data.Bernard))
                    .And
                    .ChasseurATiré(Data.Bernard, ballesRestantes: 7)
                    .And
                    .GalinettesSurLeTerrain(3);
            });
        }

        public class Echoue : UseCaseTest<UseCases.Tirer, VoidResponse>
        {
            public Echoue() : base((r, p) => new UseCases.Tirer(r, TimeProvider))
            {
            }

            [Fact]
            public void CarPartieNexistePasSansException()
            {
                Given(UnePartieDeChasseInexistante());

                When(partieDeChasseId =>
                    UseCase.Handle(new Domain.Tirer.Tirer(partieDeChasseId, Data.Bernard)));

                ThenFailWith(
                    $"La partie de chasse {PartieDeChasseId} n'existe pas",
                    savedPartieDeChasse => savedPartieDeChasse.Should().BeNull()
                );
            }

            [Fact]
            public async Task CarLeChasseurNestPasDansLaPartie()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                    ));

                When(id => UseCase.Handle(new Domain.Tirer.Tirer(id, Data.ChasseurInconnu)));

                ThenFailWith($"Chasseur inconnu Chasseur inconnu",
                    savedPartieDeChasse => savedPartieDeChasse
                        .Should()
                        .HaveEmittedEvent(Repository,
                            new ChasseurInconnuAVouluTiré(savedPartieDeChasse!.Id, Now, Data.ChasseurInconnu))
                );
            }

            [Fact]
            public async Task AvecUnChasseurNayantPlusDeBalles()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Avec(Dédé(), Bernard().SansBalles(), Robert())
                    ));

                When(id => UseCase.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));

                ThenFailWith("Bernard tire -> T'as plus de balles mon vieux, chasse à la main",
                    savedPartieDeChasse => savedPartieDeChasse
                        .Should()
                        .HaveEmittedEvent(Repository,
                            new ChasseurSansBallesAVouluTiré(savedPartieDeChasse!.Id, Now, Data.Bernard, "tire"))
                );
            }

            [Fact]
            public async Task SiLesChasseursSontEnApero()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .ALapéro()
                    ));

                When(id => UseCase.Handle(new Domain.Tirer.Tirer(id, Data.ChasseurInconnu)));

                ThenFailWith("Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!",
                    savedPartieDeChasse =>
                        savedPartieDeChasse
                            .Should()
                            .HaveEmittedEvent(Repository,
                                new ChasseurAVouluTiréPendantLApéro(savedPartieDeChasse!.Id, Now, Data.ChasseurInconnu))
                );
            }

            [Fact]
            public async Task SiLaPartieDeChasseEstTerminée()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Terminée()
                    ));

                When(id => UseCase.Handle(new Domain.Tirer.Tirer(id, Data.ChasseurInconnu)));

                ThenFailWith("Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée",
                    savedPartieDeChasse =>
                        savedPartieDeChasse
                            .Should()
                            .HaveEmittedEvent(Repository,
                                new ChasseurAVouluTiréQuandPartieTerminée(savedPartieDeChasse!.Id, Now,
                                    Data.ChasseurInconnu))
                );
            }
        }
    }
}