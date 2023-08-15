using Bouchonnois.Domain;
using Bouchonnois.Domain.Commands;

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

        public string Handle(Domain.Commands.TerminerLaPartie terminerLaPartie)
        {
            // TODO : missing null check here
            var partieDeChasse = _repository.GetById(terminerLaPartie.PartieDeChasseId);
            var result = partieDeChasse.Terminer(_timeProvider);

            _repository.Save(partieDeChasse);

            return result;
        }
    }
}