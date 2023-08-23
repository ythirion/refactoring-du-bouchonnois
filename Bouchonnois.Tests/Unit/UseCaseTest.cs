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
        protected static IEnumerable<(string, int)> PasDeChasseurs => new List<(string, int)>();
    }

    public abstract class UseCaseTest<TUseCase, TSuccessResponse> : UseCaseTest
    {
        protected readonly PartieDeChasseRepositoryForTests Repository;
        protected readonly TUseCase UseCase;
        protected Guid PartieDeChasseId;

        protected UseCaseTest(Func<IPartieDeChasseRepository, Func<DateTime>, TUseCase> useCaseFactory)
        {
            Repository = new PartieDeChasseRepositoryForTests(new InMemoryEventStore(TimeProvider));
            UseCase = useCaseFactory(Repository, TimeProvider);
        }

        protected async Task<PartieDeChasse> UnePartieDeChasseExistante(PartieDeChasseBuilder partieDeChasseBuilder)
        {
            var partieDeChasse = partieDeChasseBuilder.Build(TimeProvider);
            await Repository.Save(partieDeChasse);

            return partieDeChasse;
        }

        private PartieDeChasse? partieDeChasse() => Repository.partieDeChasse();

        #region Given / When / Then DSL

        protected void Given(Guid partieDeChasseId) => PartieDeChasseId = partieDeChasseId;
        protected void Given(PartieDeChasse unePartieDeChasseExistante) => Given(unePartieDeChasseExistante.Id);

        private Func<Guid, EitherAsync<Error, TSuccessResponse>>? _act;
        protected void When(Func<Guid, EitherAsync<Error, TSuccessResponse>>? act) => _act = act;

        protected void Then(Action<TSuccessResponse, PartieDeChasse?> assert)
        {
            var result = _act!(PartieDeChasseId);
            result.Should().BeRight();
            result.IfRight(r => assert(r, partieDeChasse()));
        }

        protected void ThenFailWith(string expectedErrorMessage,
            Action<PartieDeChasse?>? assertpartieDeChasse = null)
        {
            var result = _act!(PartieDeChasseId);
            result.Should().BeLeft();
            result.IfLeft(r =>
            {
                r.Message.Should().Be(expectedErrorMessage);
                assertpartieDeChasse?.Invoke(partieDeChasse());
            });
        }

        protected async Task<bool> FailWith(
            string errorMessage,
            Func<EitherAsync<Error, TSuccessResponse>> func,
            Func<PartieDeChasse?, bool>? assert = null)
        {
            var result = await func().AsTask();

            if (await result.IsLeft)
            {
                return result.LeftUnsafe().Message == errorMessage
                       && (assert?.Invoke(partieDeChasse()) ?? true);
            }

            return false;
        }

        #endregion
    }
}