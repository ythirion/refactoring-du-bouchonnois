using Bouchonnois.Domain;
using Bouchonnois.Domain.Démarrer;
using Bouchonnois.Tests.Builders;
using Domain.Core;
using FsCheck;
using FsCheck.Xunit;
using static Bouchonnois.Tests.Builders.CommandBuilder;
using static Bouchonnois.Tests.Unit.Generators;
using static FsCheck.Prop;
using Chasseur = Bouchonnois.Domain.Démarrer.Chasseur;
using DemarrerPartieDeChasse = Bouchonnois.UseCases.DemarrerPartieDeChasse;

namespace Bouchonnois.Tests.Unit
{
    [UsesVerify]
    public class DemarrerUnePartieDeChasse : UseCaseTest<DemarrerPartieDeChasse, Guid>
    {
        public DemarrerUnePartieDeChasse() : base((r, p) => new DemarrerPartieDeChasse(r, p))
        {
        }

        [Fact]
        public Task AvecPlusieursChasseurs()
        {
            var command = DémarrerUnePartieDeChasse()
                .Avec((Data.Dédé, 20), (Data.Bernard, 8), (Data.Robert, 12))
                .SurUnTerrainRicheEnGalinettes()
                .Build();

            UseCase.Handle(command);

            return Verify(Repository.LastEvent())
                .DontScrubDateTimes();
        }

        [Property]
        public Property Sur1TerrainAvecGalinettesEtChasseursAvecTousDesBalles() =>
            ForAll(TerrainRicheEnGalinettesGenerator(),
                ChasseursAvecBallesGenerator(),
                (terrain, chasseurs) => DémarreLaPartieAvecSuccès(terrain, chasseurs));

        private bool DémarreLaPartieAvecSuccès((string nom, int nbGalinettes) terrain,
            IEnumerable<(string nom, int nbBalles)> chasseurs)
            => UseCase
                .Handle(ToCommand(terrain, chasseurs))
                .RightUnsafe() == Repository.partieDeChasse()!.Id;

        private static Domain.Démarrer.DemarrerPartieDeChasse ToCommand((string nom, int nbGalinettes) terrain,
            IEnumerable<(string nom, int nbBalles)> chasseurs)
        {
            return new Domain.Démarrer.DemarrerPartieDeChasse(
                new TerrainDeChasse(terrain.nom, terrain.nbGalinettes),
                chasseurs.Select(c => new Chasseur(c.nom, c.nbBalles)));
        }

        public class Echoue : UseCaseTest<DemarrerPartieDeChasse, Guid>
        {
            public Echoue() : base((r, p) => new DemarrerPartieDeChasse(r, p))
            {
            }

            [Property]
            public Property SansChasseursSurNimporteQuelTerrainRicheEnGalinette()
                => ForAll(
                    TerrainRicheEnGalinettesGenerator(),
                    terrain =>
                        EchoueAvec(
                            "Impossible de démarrer une partie de chasse sans chasseurs...",
                            terrain,
                            PasDeChasseurs,
                            partieDeChasse => partieDeChasse == null)
                );

            [Property]
            public Property AvecUnTerrainSansGalinettes()
                => ForAll(
                    TerrainSansGalinettesGenerator(),
                    ChasseursAvecBallesGenerator(),
                    (terrain, chasseurs) =>
                        EchoueAvec(
                            "Impossible de démarrer une partie de chasse sur un terrain sans galinettes",
                            terrain,
                            chasseurs.ToList(),
                            partieDeChasse => partieDeChasse == null)
                );

            [Property]
            public Property SiAuMoins1ChasseurSansBalle() =>
                ForAll(
                    TerrainRicheEnGalinettesGenerator(),
                    AuMoins1ChasseurSansBalle(),
                    (terrain, chasseurs) =>
                        EchoueAvec(
                            "Impossible de démarrer une partie de chasse avec un chasseur sans balle(s)...",
                            terrain,
                            chasseurs.ToList(),
                            partieDeChasse => partieDeChasse == null)
                );


            private bool EchoueAvec(
                string message,
                (string nom, int nbGalinettes) terrain,
                IEnumerable<(string nom, int nbBalles)> chasseurs,
                Func<PartieDeChasse?, bool>? assert = null) =>
                AsyncHelper.RunSync(
                    () => FailWith(message, () => UseCase.Handle(ToCommand(terrain, chasseurs)), assert));
        }
    }
}