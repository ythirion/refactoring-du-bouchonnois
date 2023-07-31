using Bouchonnois.Domain;
using Bouchonnois.UseCases.Exceptions;

namespace Bouchonnois.UseCases
{
    public sealed class TirerSurUneGalinette
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly Func<DateTime> _timeProvider;

        public TirerSurUneGalinette(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
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

            partieDeChasse.TirerSurUneGalinette(chasseur, _timeProvider, _repository);

            _repository.Save(partieDeChasse);
        }
    }
}