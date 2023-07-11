using Bouchonnois.Domain;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Builders;
using Bouchonnois.UseCases;
using FsCheck;
using FsCheck.Xunit;
using static Bouchonnois.Tests.Builders.CommandBuilder;
using static FsCheck.Prop;

namespace Bouchonnois.Tests.Unit
{
    [UsesVerify]
    public class DemarrerUnePartieDeChasse : UseCaseTest<DemarrerPartieDeChasse>
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

            _useCase.Handle(
                command.Terrain,
                command.Chasseurs
            );

            return Verify(Repository.SavedPartieDeChasse())
                .DontScrubDateTimes();
        }

        [Property]
        public Property Sur1TerrainAvecGalinettesEtChasseursAvecTousDesBalles() =>
            ForAll(
                terrainRicheEnGalinettesGenerator(),
                chasseursAvecBallesGenerator(),
                (terrain, chasseurs) => DémarreLaPartieAvecSuccès(terrain, chasseurs));

        private bool DémarreLaPartieAvecSuccès((string nom, int nbGalinettes) terrain,
            IEnumerable<(string nom, int nbBalles)> chasseurs)
            => _useCase.Handle(
                terrain,
                chasseurs.ToList()) == Repository.SavedPartieDeChasse()!.Id;

        public class Echoue : UseCaseTest<DemarrerPartieDeChasse>
        {
            public Echoue() : base((r, p) => new DemarrerPartieDeChasse(r, p))
            {
            }

            [Property]
            public Property SansChasseursSurNimporteQuelTerrainRicheEnGalinette()
                => ForAll(
                    terrainRicheEnGalinettesGenerator(),
                    terrain =>
                        EchoueAvec<ImpossibleDeDémarrerUnePartieSansChasseur>(
                            terrain,
                            PasDeChasseurs,
                            savedPartieDeChasse => savedPartieDeChasse == null)
                );

            [Property]
            public Property AvecUnTerrainSansGalinettes()
                => ForAll(
                    terrainSansGalinettesGenerator(),
                    chasseursAvecBallesGenerator(),
                    (terrain, chasseurs) =>
                        EchoueAvec<ImpossibleDeDémarrerUnePartieSansGalinettes>(
                            terrain,
                            chasseurs.ToList(),
                            savedPartieDeChasse => savedPartieDeChasse == null)
                );

            [Property]
            public Property SiAuMoins1ChasseurSansBalle() =>
                ForAll(
                    terrainRicheEnGalinettesGenerator(),
                    chasseursSansBallesGenerator(),
                    (terrain, chasseurs) =>
                        EchoueAvec<ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle>(
                            terrain,
                            chasseurs.ToList(),
                            savedPartieDeChasse => savedPartieDeChasse == null)
                );

            private bool EchoueAvec<TException>(
                (string nom, int nbGalinettes) terrain,
                IEnumerable<(string nom, int nbBalles)> chasseurs,
                Func<PartieDeChasse?, bool>? assert = null) where TException : Exception
                => MustFailWith<TException>(() => _useCase.Handle(terrain, chasseurs.ToList()), assert);
        }
    }
}