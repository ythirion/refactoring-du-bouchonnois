using Bouchonnois.Domain.Tirer;
using Bouchonnois.Tests.Builders;
using Bouchonnois.UseCases;

namespace Bouchonnois.Tests.Unit
{
    public class Tirer : UseCaseTest<UseCases.Tirer, VoidResponse>
    {
        public Tirer() : base((r, p) => new UseCases.Tirer(r))
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

            await When(id => UseCase.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));


            await Then((response, partieDeChasse) =>
            {
                response.Should().Be(VoidResponse.Empty);
                partieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository, new ChasseurATiré(partieDeChasse!.Id, Now, Data.Bernard));
            });
        }

        public class Echoue : UseCaseTest<UseCases.Tirer, VoidResponse>
        {
            public Echoue() : base((r, p) => new UseCases.Tirer(r))
            {
            }

            [Fact]
            public async Task CarPartieNexistePasSansException()
            {
                Given(UnePartieDeChasseInexistante());

                await When(partieDeChasseId =>
                    UseCase.Handle(new Domain.Tirer.Tirer(partieDeChasseId, Data.Bernard)));

                await ThenFailWith(
                    $"La partie de chasse {PartieDeChasseId} n'existe pas",
                    partieDeChasse => partieDeChasse.Should().BeNull()
                );
            }

            [Fact]
            public async Task CarLeChasseurNestPasDansLaPartie()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                    ));

                await When(id => UseCase.Handle(new Domain.Tirer.Tirer(id, Data.ChasseurInconnu)));

                await ThenFailWith($"Chasseur inconnu Chasseur inconnu",
                    partieDeChasse => partieDeChasse
                        .Should()
                        .HaveEmittedEvent(Repository,
                            new ChasseurInconnuAVouluTiré(partieDeChasse!.Id, Now, Data.ChasseurInconnu))
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

                await When(id => UseCase.Handle(new Domain.Tirer.Tirer(id, Data.Bernard)));

                await ThenFailWith("Bernard tire -> T'as plus de balles mon vieux, chasse à la main",
                    partieDeChasse => partieDeChasse
                        .Should()
                        .HaveEmittedEvent(Repository,
                            new ChasseurSansBallesAVouluTiré(partieDeChasse!.Id, Now, Data.Bernard, "tire"))
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

                await When(id => UseCase.Handle(new Domain.Tirer.Tirer(id, Data.ChasseurInconnu)));

                await ThenFailWith("Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!",
                    partieDeChasse =>
                        partieDeChasse
                            .Should()
                            .HaveEmittedEvent(Repository,
                                new ChasseurAVouluTiréPendantLApéro(partieDeChasse!.Id, Now, Data.ChasseurInconnu))
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

                await When(id => UseCase.Handle(new Domain.Tirer.Tirer(id, Data.ChasseurInconnu)));

                await ThenFailWith("Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée",
                    partieDeChasse =>
                        partieDeChasse
                            .Should()
                            .HaveEmittedEvent(Repository,
                                new ChasseurAVouluTiréQuandPartieTerminée(partieDeChasse!.Id, Now,
                                    Data.ChasseurInconnu))
                );
            }
        }
    }
}