using Bouchonnois.Domain;
using Bouchonnois.Domain.Commands;
using Bouchonnois.Tests.Builders;
using FsCheck;
using FsCheck.Xunit;
using static Bouchonnois.Tests.Builders.CommandBuilder;
using static Bouchonnois.Tests.Unit.Generators;
using static FsCheck.Prop;
using Chasseur = Bouchonnois.Domain.Commands.Chasseur;
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
                .SurUnTerrainRicheEnGalinettes();

            _useCase.Handle(command.Build());

            return Verify(Repository.SavedPartieDeChasse())
                .DontScrubDateTimes();
        }

        [Property]
        public Property Sur1TerrainAvecGalinettesEtChasseursAvecTousDesBalles() =>
            ForAll(TerrainRicheEnGalinettesGenerator(),
                ChasseursAvecBallesGenerator(),
                (terrain, chasseurs) => DémarreLaPartieAvecSuccès(terrain, chasseurs));

        private bool DémarreLaPartieAvecSuccès((string nom, int nbGalinettes) terrain,
            IEnumerable<(string nom, int nbBalles)> chasseurs)
            => _useCase.Handle(ToCommand(terrain, chasseurs))
                .RightUnsafe() == Repository.SavedPartieDeChasse()!.Id;

        private static Domain.Commands.DemarrerPartieDeChasse ToCommand((string nom, int nbGalinettes) terrain,
            IEnumerable<(string nom, int nbBalles)> chasseurs)
        {
            return new Domain.Commands.DemarrerPartieDeChasse(
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
                            terrain,
                            PasDeChasseurs,
                            savedPartieDeChasse => savedPartieDeChasse == null)
                );

            [Property]
            public Property AvecUnTerrainSansGalinettes()
                => ForAll(
                    TerrainSansGalinettesGenerator(),
                    ChasseursAvecBallesGenerator(),
                    (terrain, chasseurs) =>
                        EchoueAvec(
                            terrain,
                            chasseurs.ToList(),
                            savedPartieDeChasse => savedPartieDeChasse == null)
                );

            [Property]
            public Property SiAuMoins1ChasseurSansBalle() =>
                ForAll(
                    TerrainRicheEnGalinettesGenerator(),
                    AuMoins1ChasseurSansBalle(),
                    (terrain, chasseurs) =>
                        EchoueAvec(
                            terrain,
                            chasseurs.ToList(),
                            savedPartieDeChasse => savedPartieDeChasse == null)
                );

            private bool EchoueAvec(
                (string nom, int nbGalinettes) terrain,
                IEnumerable<(string nom, int nbBalles)> chasseurs,
                Func<PartieDeChasse?, bool>? assert = null) =>
                FailWith(() => _useCase.Handle(ToCommand(terrain, chasseurs)), assert);
        }
    }
}