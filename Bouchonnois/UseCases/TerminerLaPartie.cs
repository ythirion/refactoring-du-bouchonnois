using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class TerminerLaPartie
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly Func<DateTime> _timeProvider;

        public TerminerLaPartie(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
        {
            _repository = repository;
            _timeProvider = timeProvider;
        }

        public string Handle(Guid id)
        {
            // TODO : missing null check here
            var partieDeChasse = _repository.GetById(id);
            var result = partieDeChasse.Terminer(_timeProvider);

            _repository.Save(partieDeChasse);

            return result;
        }
    }
}