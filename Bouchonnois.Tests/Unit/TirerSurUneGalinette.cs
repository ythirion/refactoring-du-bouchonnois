using Bouchonnois.Domain.Tirer;
using Bouchonnois.Tests.Builders;
using Bouchonnois.UseCases;

namespace Bouchonnois.Tests.Unit
{
    public class TirerSurUneGalinette : UseCaseTest<UseCases.TirerSurUneGalinette, VoidResponse>
    {
        public TirerSurUneGalinette() : base((r, p) => new UseCases.TirerSurUneGalinette(r))
        {
        }

        [Fact]
        public async Task AvecUnChasseurAyantDesBallesEtAssezDeGalinettesSurLeTerrain()
        {
            Given(
                await UnePartieDeChasseExistante(
                    SurUnTerrainRicheEnGalinettes()
                ));

            When(id => UseCase.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.Bernard)));

            Then((response, partieDeChasse) =>
            {
                response.Should().Be(VoidResponse.Empty);
                partieDeChasse
                    .Should()
                    .HaveEmittedEvent(Repository,
                        new ChasseurATiréSurUneGalinette(partieDeChasse!.Id, Now, Data.Bernard));
            });
        }

        public class Echoue : UseCaseTest<UseCases.TirerSurUneGalinette, VoidResponse>
        {
            public Echoue() : base((r, p) => new UseCases.TirerSurUneGalinette(r))
            {
            }

            [Fact]
            public void CarPartieNexistePas()
            {
                Given(UnePartieDeChasseInexistante());

                When(id => UseCase.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.Bernard)));

                ThenFailWith(
                    $"La partie de chasse {PartieDeChasseId} n'existe pas",
                    partieDeChasse => partieDeChasse.Should().BeNull()
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

                When(id => UseCase.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.Bernard)));

                ThenFailWith("Bernard veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main",
                    partieDeChasse => partieDeChasse
                        .Should()
                        .HaveEmittedEvent(Repository,
                            new ChasseurSansBallesAVouluTiré(partieDeChasse!.Id, Now, Data.Bernard,
                                "veut tirer sur une galinette")
                        ));
            }

            [Fact]
            public async Task CarPlusDeGalinetteSurLeTerrain()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes(1)
                            .Avec(Dédé().AyantTué(1), Robert())
                    ));

                When(id => UseCase.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.Robert)));

                ThenFailWith("Robert, t'as trop picolé mon vieux, t'as rien touché",
                    partieDeChasse => partieDeChasse.Should().HaveEmittedEvent(
                        Repository,
                        new ChasseurACruTiréSurGalinette(partieDeChasse!.Id, Now, Data.Robert))
                );
            }

            [Fact]
            public async Task CarLeChasseurNestPasDansLaPartie()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Avec(Dédé(), Robert())
                    ));

                When(id => UseCase.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.ChasseurInconnu)));

                ThenFailWith($"Chasseur inconnu Chasseur inconnu",
                    partieDeChasse => partieDeChasse
                        .Should()
                        .HaveEmittedEvent(Repository,
                            new ChasseurInconnuAVouluTiré(partieDeChasse!.Id, Now, Data.ChasseurInconnu))
                );
            }

            [Fact]
            public async Task SiLesChasseursSontEnApero()
            {
                Given(
                    await UnePartieDeChasseExistante(
                        SurUnTerrainRicheEnGalinettes()
                            .Avec(Dédé(), Robert())
                            .ALapéro()
                    ));

                When(id =>
                    UseCase.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.ChasseurInconnu)));

                ThenFailWith("Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!",
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
                            .Avec(Dédé(), Robert())
                            .Terminée()
                    ));

                When(id => UseCase.Handle(new Domain.Tirer.TirerSurUneGalinette(id, Data.ChasseurInconnu)));

                ThenFailWith("Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée",
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