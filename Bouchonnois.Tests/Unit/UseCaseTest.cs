using Bouchonnois.Domain;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;
using Domain.Core;
using FluentAssertions.LanguageExt;
using LanguageExt;

namespace Bouchonnois.Tests.Unit
{
    public abstract class UseCaseTest
    {
        protected static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
        protected static readonly Func<DateTime> TimeProvider = () => Now;
        protected static List<(string, int)> PasDeChasseurs => new();
    }

    public abstract class UseCaseTest<TUseCase, TSuccessResponse> : UseCaseTest
    {
        protected readonly PartieDeChasseRepositoryForTests Repository;
        protected readonly TUseCase _useCase;
        protected Guid _partieDeChasseId;

        protected UseCaseTest(Func<IPartieDeChasseRepository, Func<DateTime>, TUseCase> useCaseFactory)
        {
            Repository = new PartieDeChasseRepositoryForTests(new InMemoryEventStore(TimeProvider));
            _useCase = useCaseFactory(Repository, TimeProvider);
        }

        protected PartieDeChasse UnePartieDeChasseExistante(PartieDeChasseBuilder partieDeChasseBuilder)
        {
            var partieDeChasse = partieDeChasseBuilder.Build(TimeProvider, Repository);
            Repository.Add(partieDeChasse);

            return partieDeChasse;
        }

        protected PartieDeChasse? SavedPartieDeChasse() => Repository.SavedPartieDeChasse();

        #region Given / When / Then DSL

        protected void Given(Guid partieDeChasseId) => _partieDeChasseId = partieDeChasseId;
        protected void Given(PartieDeChasse unePartieDeChasseExistante) => Given(unePartieDeChasseExistante.Id);

        private Func<Guid, Either<Error, TSuccessResponse>>? _act;
        protected void When(Func<Guid, Either<Error, TSuccessResponse>>? act) => _act = act;

        protected void Then(Action<TSuccessResponse, PartieDeChasse?> assert)
        {
            var result = _act!(_partieDeChasseId);
            result.Should().BeRight();
            result.IfRight(r => assert(r, SavedPartieDeChasse()));
        }

        protected void ThenFailWith(string expectedErrorMessage,
            Action<PartieDeChasse?>? assertSavedPartieDeChasse = null)
        {
            var result = _act!(_partieDeChasseId);
            result.Should().BeLeft();
            result.IfLeft(r =>
            {
                r.Message.Should().Be(expectedErrorMessage);
                assertSavedPartieDeChasse?.Invoke(SavedPartieDeChasse());
            });
        }

        protected bool FailWith(
            string errorMessage,
            Func<Either<Error, TSuccessResponse>> func,
            Func<PartieDeChasse?, bool>? assert = null)
        {
            var result = func();

            if (result.IsLeft)
            {
                return result.LeftUnsafe().Message == errorMessage
                       && (assert?.Invoke(SavedPartieDeChasse()) ?? true);
            }

            return false;
        }

        #endregion
    }
}