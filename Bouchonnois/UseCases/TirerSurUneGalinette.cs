using Bouchonnois.Domain;
using Bouchonnois.Domain.Commands;
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

        public void Handle(Domain.Commands.TirerSurUneGalinette tirerSurUneGalinette)
        {
            var partieDeChasse = _repository.GetById(tirerSurUneGalinette.PartieDeChasseId);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            partieDeChasse.TirerSurUneGalinette(tirerSurUneGalinette.Chasseur, _timeProvider, _repository);

            _repository.Save(partieDeChasse);
        }
    }
}