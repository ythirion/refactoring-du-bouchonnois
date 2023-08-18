using Bouchonnois.Domain;

namespace Bouchonnois.Tests.Unit
{
    public abstract partial class UseCaseTest<TUseCase>
    {
        private Action<Guid>? _actWithException;

        protected void WhenWithException(Action<Guid> act) => _actWithException = act;

        protected void ThenWithException(Action<PartieDeChasse?> assert, Action? assertResult = null)
        {
            _actWithException!(_partieDeChasseId);
            assert(SavedPartieDeChasse());
            assertResult?.Invoke();
        }

        protected void ThenThrow<TException>(Action<PartieDeChasse?> assert, string? expectedMessage = null)
            where TException : Exception
        {
            var ex = ((Action) (() => _actWithException!(_partieDeChasseId))).Should().Throw<TException>();
            if (expectedMessage is not null) ex.WithMessage(expectedMessage);

            assert(SavedPartieDeChasse());
        }
    }
}