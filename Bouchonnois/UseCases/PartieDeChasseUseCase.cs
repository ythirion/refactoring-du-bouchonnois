using Bouchonnois.Domain;
using Bouchonnois.Domain.Commands;
using LanguageExt;
using static Bouchonnois.Domain.Error;

namespace Bouchonnois.UseCases
{
    public abstract class PartieDeChasseUseCase<TRequest, TResponse> : IUseCase<TRequest, TResponse>
        where TRequest : PartieDeChasseCommand
    {
        protected readonly IPartieDeChasseRepository _repository;
        private readonly Func<PartieDeChasse, TRequest, Either<Error, TResponse>> _handler;

        protected PartieDeChasseUseCase(IPartieDeChasseRepository repository,
            Func<PartieDeChasse, TRequest, Either<Error, TResponse>> handler
        )
        {
            _repository = repository;
            _handler = handler;
        }

        public Either<Error, TResponse> Handle(TRequest command)
        {
            PartieDeChasse? foundPartieDeChasse = null;

            var result = _repository
                .GetByIdOption(command.PartieDeChasseId)
                .ToEither(() => AnError($"La partie de chasse {command.PartieDeChasseId} n'existe pas"))
                .Do(p => foundPartieDeChasse = p)
                .Bind(p => _handler(p, command));

            if (foundPartieDeChasse != null) _repository.Save(foundPartieDeChasse);

            return result;
        }
    }
}