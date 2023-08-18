using Bouchonnois.Domain;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;
using FluentAssertions.LanguageExt;
using LanguageExt;

namespace Bouchonnois.Tests.Unit
{
    public abstract class UseCaseTestBase
    {
        protected static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
        protected static readonly Func<DateTime> TimeProvider = () => Now;
        protected static List<(string, int)> PasDeChasseurs => new();
    }

    public abstract partial class UseCaseTest<TUseCase> : UseCaseTestBase
    {
        protected readonly PartieDeChasseRepositoryForTests Repository;
        protected readonly TUseCase _useCase;
        protected Guid _partieDeChasseId;

        protected UseCaseTest(Func<IPartieDeChasseRepository, Func<DateTime>, TUseCase> useCaseFactory)
        {
            Repository = new PartieDeChasseRepositoryForTests();
            _useCase = useCaseFactory(Repository, TimeProvider);
        }

        protected PartieDeChasse UnePartieDeChasseExistante(PartieDeChasseBuilder partieDeChasseBuilder)
        {
            var partieDeChasse = partieDeChasseBuilder.Build(TimeProvider, Repository);
            Repository.Add(partieDeChasse);

            return partieDeChasse;
        }

        protected PartieDeChasse? SavedPartieDeChasse() => Repository.SavedPartieDeChasse();

        protected void Given(Guid partieDeChasseId) => _partieDeChasseId = partieDeChasseId;
        protected void Given(PartieDeChasse unePartieDeChasseExistante) => Given(unePartieDeChasseExistante.Id);

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
    }

    public abstract class UseCaseTestWithoutException<TUseCase, TSuccessResponse> : UseCaseTest<TUseCase>
    {
        protected UseCaseTestWithoutException(Func<IPartieDeChasseRepository, Func<DateTime>, TUseCase> useCaseFactory)
            : base(useCaseFactory)
        {
        }

        private Func<Guid, Either<Error, TSuccessResponse>>? _act;
        protected void When(Func<Guid, Either<Error, TSuccessResponse>>? act) => _act = act;

        protected void ThenFailWith(string expectedErrorMessage, Action<PartieDeChasse?>? assertSavedPartieDeChasse)
        {
            var result = _act!(_partieDeChasseId);
            result.Should().BeLeft();
            result.IfLeft(r =>
            {
                r.Message.Should().Be(expectedErrorMessage);
                assertSavedPartieDeChasse?.Invoke(SavedPartieDeChasse());
            });
        }
    }
}