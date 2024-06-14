using Bouchonnois.Domain;

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

        public Guid Handle((string nom, int nbGalinettes) terrainDeChasse, List<(string nom, int nbBalles)> chasseurs)
        {
            var partieDeChasse = PartieDeChasse.Create(_timeProvider, terrainDeChasse, chasseurs);
            _repository.Save(partieDeChasse);

            return partieDeChasse.Id;
        }
    }
}