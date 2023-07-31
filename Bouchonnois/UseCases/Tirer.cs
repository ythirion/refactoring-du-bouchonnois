using Bouchonnois.Domain;
using Bouchonnois.UseCases.Exceptions;

namespace Bouchonnois.UseCases
{
    public sealed class Tirer
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly Func<DateTime> _timeProvider;

        public Tirer(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
        {
            _repository = repository;
            _timeProvider = timeProvider;
        }

        public void Handle(Guid id, string chasseur)
        {
            var partieDeChasse = _repository.GetById(id);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            partieDeChasse.Tirer(chasseur, _timeProvider, _repository);
            _repository.Save(partieDeChasse);
        }
    }
}