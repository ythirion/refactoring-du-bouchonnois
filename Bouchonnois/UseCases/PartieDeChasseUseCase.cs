using Bouchonnois.Domain;
using Bouchonnois.Domain.Commands;
using Bouchonnois.UseCases.Exceptions;
using static Bouchonnois.UseCases.VoidResponse;

namespace Bouchonnois.UseCases
{
    public abstract class PartieDeChasseUseCase<TRequest, TResponse> : IUseCase<TRequest, TResponse>
        where TRequest : PartieDeChasseCommand
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly Func<PartieDeChasse, TRequest, TResponse> _domainHandler;

        protected PartieDeChasseUseCase(IPartieDeChasseRepository repository,
            Func<PartieDeChasse, TRequest, TResponse> domainHandler
        )
        {
            _repository = repository;
            _domainHandler = domainHandler;
        }

        public TResponse Handle(TRequest command)
        {
            var partieDeChasse = _repository.GetById(command.PartieDeChasseId);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            var response = _domainHandler(partieDeChasse, command);
            _repository.Save(partieDeChasse);

            return response;
        }
    }

    public abstract class EmptyResponsePartieDeChasseUseCase<TRequest> : PartieDeChasseUseCase<TRequest, VoidResponse>
        where TRequest : PartieDeChasseCommand
    {
        protected EmptyResponsePartieDeChasseUseCase(IPartieDeChasseRepository repository,
            Action<PartieDeChasse, TRequest> domainHandler)
            : base(repository,
                (partieDeChasse, command) =>
                {
                    domainHandler(partieDeChasse, command);
                    return Empty;
                }
            )
        {
        }
    }
}