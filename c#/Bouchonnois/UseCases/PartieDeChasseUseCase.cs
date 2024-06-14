using Bouchonnois.Domain;
using Bouchonnois.Domain.Commands;
using LanguageExt;
using static Bouchonnois.Domain.Error;

namespace Bouchonnois.UseCases
{
    public abstract class PartieDeChasseUseCase<TRequest, TResponse> : IUseCase<TRequest, TResponse>
        where TRequest : PartieDeChasseCommand
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly Func<PartieDeChasse, TRequest, Either<Error, TResponse>> _handler;

        protected PartieDeChasseUseCase(
            IPartieDeChasseRepository repository,
            Func<PartieDeChasse, TRequest, Either<Error, TResponse>> handler)
        {
            _repository = repository;
            _handler = handler;
        }

        public Either<Error, TResponse> Handle(TRequest command) =>
            _repository
                .GetById(command.PartieDeChasseId)
                .ToEither(() => AnError($"La partie de chasse {command.PartieDeChasseId} n'existe pas"))
                .Bind(p => HandleCommand(p, command));

        private Either<Error, TResponse> HandleCommand(PartieDeChasse partieDeChasse, TRequest command)
            => _handler(partieDeChasse, command)
                .Let(_ => _repository.Save(partieDeChasse));

        protected static Either<Error, VoidResponse> ToEmpty(Either<Error, PartieDeChasse> either)
            => either.Map(_ => VoidResponse.Empty);
    }
}