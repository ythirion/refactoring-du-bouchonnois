using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Builders;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.FSharp.Collections;
using static Bouchonnois.Tests.Builders.CommandBuilder;

namespace Bouchonnois.Tests.Unit
{
    [UsesVerify]
    public class DemarrerUnePartieDeChasse : PartieDeChasseServiceTest
    {
        [Fact]
        public Task AvecPlusieursChasseurs()
        {
            var command = DémarrerUnePartieDeChasse()
                .Avec((Data.Dédé, 20), (Data.Bernard, 8), (Data.Robert, 12))
                .SurUnTerrainRicheEnGalinettes();

            PartieDeChasseService.Demarrer(
                command.Terrain,
                command.Chasseurs
            );

            return Verify(Repository.SavedPartieDeChasse())
                .DontScrubDateTimes();
        }

        private static Arbitrary<(string nom, int nbGalinettes)> terrainRicheEnGalinettesGenerator()
            => (from nom in Arb.Generate<string>()
                from nbGalinette in Gen.Choose(1, int.MaxValue)
                select (nom, nbGalinette)).ToArbitrary();

        private static Arbitrary<(string nom, int nbBalles)> chasseurAvecDesBallesGenerator()
            => (from nom in Arb.Generate<string>()
                from nbBalles in Gen.Choose(1, int.MaxValue)
                select (nom, nbBalles)).ToArbitrary();

        private static Arbitrary<FSharpList<(string nom, int nbBalles)>> groupeDeChasseursGenerator()
            => (from nbChasseurs in Gen.Choose(1, 1_000)
                select chasseurAvecDesBallesGenerator().Generator.Sample(1, nbChasseurs)).ToArbitrary();

        [Property]
        public Property Sur1TerrainAvecGalinettesEtAuMoins1ChasseurAvecTousDesBalles() =>
            Prop.ForAll(
                terrainRicheEnGalinettesGenerator(),
                groupeDeChasseursGenerator(),
                (terrain, chasseurs) => DémarreLaPartieAvecSuccès(terrain, chasseurs));

        private bool DémarreLaPartieAvecSuccès((string nom, int nbGalinettes) terrain,
            IEnumerable<(string nom, int nbBalles)> chasseurs)
            => PartieDeChasseService.Demarrer(
                terrain,
                chasseurs.ToList()) == Repository.SavedPartieDeChasse()!.Id;

        public class Echoue : PartieDeChasseServiceTest
        {
            [Property]
            public Property SansChasseursSurNimporteQuelTerrainRicheEnGalinette()
                => Prop.ForAll(
                    terrainRicheEnGalinettesGenerator(),
                    terrain =>
                    {
                        try
                        {
                            PartieDeChasseService.Demarrer(
                                terrain,
                                new List<(string, int)>());

                            return false;
                        }
                        catch (ImpossibleDeDémarrerUnePartieSansChasseur)
                        {
                            return Repository.SavedPartieDeChasse() == null;
                        }
                    });

            [Fact]
            public void AvecUnTerrainSansGalinettes()
            {
                Action demarrerPartieSansChasseurs = () =>
                    PartieDeChasseService.Demarrer(("Pitibon sur Sauldre", 0), new List<(string, int)>());

                demarrerPartieSansChasseurs.Should()
                    .Throw<ImpossibleDeDémarrerUnePartieSansGalinettes>();
            }

            [Fact]
            public void SiChasseurSansBalle()
            {
                var command = DémarrerUnePartieDeChasse()
                    .Avec((Data.Dédé, 20), (Data.Bernard, 0))
                    .SurUnTerrainRicheEnGalinettes();

                ExecuteAndAssertThrow<ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle>(
                    s => s.Demarrer(command.Terrain, command.Chasseurs),
                    p => p.Should().BeNull());
            }
        }
    }
}