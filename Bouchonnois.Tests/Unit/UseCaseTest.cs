using Bouchonnois.Domain;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Unit
{
    public abstract class UseCaseTestBase
    {
        protected static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
        protected static readonly Func<DateTime> TimeProvider = () => Now;
        protected static List<(string, int)> PasDeChasseurs => new();
    }

    public abstract class UseCaseTest<TUseCase> : UseCaseTestBase
    {
        protected readonly PartieDeChasseRepositoryForTests Repository;
        protected readonly TUseCase _useCase;

        protected UseCaseTest(Func<IPartieDeChasseRepository, Func<DateTime>, TUseCase> useCaseFactory)
        {
            Repository = new PartieDeChasseRepositoryForTests();
            _useCase = useCaseFactory(Repository, TimeProvider);
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
    }
}