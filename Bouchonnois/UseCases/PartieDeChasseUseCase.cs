using Bouchonnois.Domain;
using Bouchonnois.Domain.Commands;
using Bouchonnois.UseCases.Exceptions;
using static Bouchonnois.UseCases.VoidResponse;

namespace Bouchonnois.UseCases
{
    public abstract class PartieDeChasseUseCase<TRequest, TResponse> : IUseCase<TRequest, TResponse>
        where TRequest : PartieDeChasseCommand
    {
        protected readonly IPartieDeChasseRepository _repository;
        private readonly Func<PartieDeChasse, TRequest, TResponse> _handler;

        protected PartieDeChasseUseCase(IPartieDeChasseRepository repository,
            Func<PartieDeChasse, TRequest, TResponse> handler
        )
        {
            _repository = repository;
            _handler = handler;
        }

        public TResponse Handle(TRequest command)
        {
            var partieDeChasse = _repository.GetById(command.PartieDeChasseId);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            var response = _handler(partieDeChasse, command);
            _repository.Save(partieDeChasse);

            return response;
        }
    }

    public abstract class EmptyResponsePartieDeChasseUseCase<TRequest> : PartieDeChasseUseCase<TRequest, VoidResponse>
        where TRequest : PartieDeChasseCommand
    {
        protected EmptyResponsePartieDeChasseUseCase(IPartieDeChasseRepository repository,
            Action<PartieDeChasse, TRequest> handler)
            : base(repository,
                (partieDeChasse, command) =>
                {
                    handler(partieDeChasse, command);
                    return Empty;
                }
            )
        {
        }
    }
}