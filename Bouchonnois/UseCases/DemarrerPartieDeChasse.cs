using Bouchonnois.Domain;
using Commands = Bouchonnois.Domain.Commands;

namespace Bouchonnois.UseCases
{
    public sealed class DemarrerPartieDeChasse
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly Func<DateTime> _timeProvider;

        public DemarrerPartieDeChasse(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
        {
            _repository = repository;
            _timeProvider = timeProvider;
        }

        public Guid Handle(Commands.DemarrerPartieDeChasse demarrerPartieDeChasse)
        {
            var partieDeChasse = PartieDeChasse.Create(_timeProvider, demarrerPartieDeChasse);
            _repository.Save(partieDeChasse);

            return partieDeChasse.Id;
        }
    }
}