using Bouchonnois.Domain;
using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.UseCases
{
    public class ConsulterStatus
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

            return string.Join(
                Environment.NewLine,
                partieDeChasse.Events
                    .OrderByDescending(@event => @event.Date)
                    .Select(@event => @event.ToString())
            );
        }
    }
}