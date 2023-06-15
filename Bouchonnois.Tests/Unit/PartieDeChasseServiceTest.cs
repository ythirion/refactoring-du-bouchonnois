using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;
using FluentAssertions.Specialized;

namespace Bouchonnois.Tests.Unit
{
    public abstract class PartieDeChasseServiceTest
    {
        protected static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
        protected static readonly Func<DateTime> TimeProvider = () => Now;

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

        private PartieDeChasse? SavedPartieDeChasse() => Repository.SavedPartieDeChasse();

        protected ExceptionAssertions<TException> ExecuteAndAssertThrow<TException>(Action<PartieDeChasseService> act,
            Action<PartieDeChasse?> assert)
            where TException : Exception
        {
            var ex = ((Action) (() => act(PartieDeChasseService))).Should().Throw<TException>();
            assert(SavedPartieDeChasse());

            return ex;
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