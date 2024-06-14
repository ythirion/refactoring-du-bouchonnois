using Bouchonnois.Domain;
using Bouchonnois.UseCases.Exceptions;

namespace Bouchonnois.UseCases
{
    public sealed class ConsulterStatus
    {
        private readonly IPartieDeChasseRepository _repository;

        public ConsulterStatus(IPartieDeChasseRepository repository)
            => _repository = repository;

        public string Handle(Guid id)
        {
            var partieDeChasse = _repository.GetById(id);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            return partieDeChasse.Consulter();
        }
    }
}