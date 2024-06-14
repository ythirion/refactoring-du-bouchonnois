using FsCheck;
using Microsoft.FSharp.Collections;
using static System.Tuple;

namespace Bouchonnois.Tests.Unit
{
    public static class Generators
    {
        private static Arbitrary<(string nom, int nbGalinettes)> TerrainGenerator(int minGalinettes, int maxGalinettes)
            => (from nom in Arb.Generate<string>()
                from nbGalinette in Gen.Frequency(
                    Create(1, Gen.Elements(minGalinettes, maxGalinettes)),
                    Create(9, Gen.Choose(minGalinettes, maxGalinettes))
                )
                //Gen.Choose(minGalinettes, maxGalinettes)
                select (nom, nbGalinette)).ToArbitrary();

        public static Arbitrary<(string nom, int nbGalinettes)> TerrainRicheEnGalinettesGenerator()
            => TerrainGenerator(1, int.MaxValue);

        public static Arbitrary<(string nom, int nbGalinettes)> TerrainSansGalinettesGenerator()
            => TerrainGenerator(-int.MaxValue, 0);

        private static Arbitrary<(string nom, int nbBalles)> ChasseursGenerator(int minBalles, int maxBalles)
            => (from nom in Arb.Generate<string>()
                from nbBalles in Gen.Choose(minBalles, maxBalles)
                select (nom, nbBalles)).ToArbitrary();

        private static Arbitrary<FSharpList<(string nom, int nbBalles)>> GroupeDeChasseursGenerator(
            int minBalles,
            int maxBalles)
            => (from nbChasseurs in Gen.Choose(1, 1_000)
                select ChasseursGenerator(minBalles, maxBalles).Generator.Sample(1, nbChasseurs)).ToArbitrary();

        public static Arbitrary<FSharpList<(string nom, int nbBalles)>> ChasseursAvecBallesGenerator()
            => GroupeDeChasseursGenerator(1, int.MaxValue);

        public static Arbitrary<FSharpList<(string nom, int nbBalles)>> AuMoins1ChasseurSansBalle()
            => GroupeDeChasseursGenerator(0, 1);
    }
}