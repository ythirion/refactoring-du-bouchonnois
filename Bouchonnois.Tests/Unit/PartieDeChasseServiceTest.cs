using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;
using FsCheck;
using Microsoft.FSharp.Collections;

namespace Bouchonnois.Tests.Unit
{
    public abstract class PartieDeChasseServiceTest
    {
        protected static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
        protected static readonly Func<DateTime> TimeProvider = () => Now;

        protected static List<(string, int)> PasDeChasseurs => new();

        protected readonly PartieDeChasseRepositoryForTests Repository;
        protected readonly PartieDeChasseService PartieDeChasseService;

        protected PartieDeChasseServiceTest()
        {
            Repository = new PartieDeChasseRepositoryForTests();
            PartieDeChasseService = new PartieDeChasseService(Repository, TimeProvider);
        }

        protected PartieDeChasse UnePartieDeChasseExistante(PartieDeChasseBuilder partieDeChasseBuilder)
        {
            var partieDeChasse = partieDeChasseBuilder.Build();
            Repository.Add(partieDeChasse);

            return partieDeChasse;
        }

        protected PartieDeChasse? SavedPartieDeChasse() => Repository.SavedPartieDeChasse();

        protected bool MustFailWith<TException>(Action action, Func<PartieDeChasse?, bool>? assert = null)
            where TException : Exception
        {
            try
            {
                action();
                return false;
            }
            catch (TException)
            {
                return assert?.Invoke(SavedPartieDeChasse()) ?? true;
            }
        }

        #region Given / When / Then DSL

        private Guid _partieDeChasseId;
        private Action<Guid>? _act;
        
        protected void Given(Guid partieDeChasseId) => _partieDeChasseId = partieDeChasseId;
        protected void Given(PartieDeChasse unePartieDeChasseExistante) => Given(unePartieDeChasseExistante.Id);
        protected void When(Action<Guid> act) => _act = act;

        protected void Then(Action<PartieDeChasse?> assert, Action? assertResult = null)
        {
            _act!(_partieDeChasseId);
            assert(SavedPartieDeChasse());
            assertResult?.Invoke();
        }

        protected void Then(Action? assertResult = null)
        {
            _act!(_partieDeChasseId);
            assertResult?.Invoke();
        }

        protected void ThenThrow<TException>(Action<PartieDeChasse?> assert, string? expectedMessage = null)
            where TException : Exception
        {
            var ex = ((Action) (() => _act!(_partieDeChasseId))).Should().Throw<TException>();
            if (expectedMessage is not null) ex.WithMessage(expectedMessage);

            assert(SavedPartieDeChasse());
        }

        #endregion

        #region Generators for properties

        private static Arbitrary<(string nom, int nbGalinettes)> terrainGenerator(int minGalinettes, int maxGalinettes)
            => (from nom in Arb.Generate<string>()
                from nbGalinette in Gen.Choose(minGalinettes, maxGalinettes)
                select (nom, nbGalinette)).ToArbitrary();

        protected static Arbitrary<(string nom, int nbGalinettes)> terrainRicheEnGalinettesGenerator()
            => terrainGenerator(1, int.MaxValue);

        protected static Arbitrary<(string nom, int nbGalinettes)> terrainSansGalinettesGenerator()
            => terrainGenerator(-int.MaxValue, 0);

        private static Arbitrary<(string nom, int nbBalles)> chasseursGenerator(int minBalles, int maxBalles)
            => (from nom in Arb.Generate<string>()
                from nbBalles in Gen.Choose(minBalles, maxBalles)
                select (nom, nbBalles)).ToArbitrary();

        private static Arbitrary<FSharpList<(string nom, int nbBalles)>> groupeDeChasseursGenerator(int minBalles,
            int maxBalles)
            => (from nbChasseurs in Gen.Choose(1, 1_000)
                select chasseursGenerator(minBalles, maxBalles).Generator.Sample(1, nbChasseurs)).ToArbitrary();

        protected static Arbitrary<FSharpList<(string nom, int nbBalles)>> chasseursAvecBallesGenerator()
            => groupeDeChasseursGenerator(1, int.MaxValue);

        protected static Arbitrary<FSharpList<(string nom, int nbBalles)>> chasseursSansBallesGenerator()
            => groupeDeChasseursGenerator(0, 0);

        #endregion
    }
}