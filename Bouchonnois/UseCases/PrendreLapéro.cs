using Bouchonnois.Domain;
using Bouchonnois.UseCases.Exceptions;

namespace Bouchonnois.UseCases
{
    public sealed class PrendreLapéro
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly Func<DateTime> _timeProvider;

        public PrendreLapéro(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
        {
            _repository = repository;
            _timeProvider = timeProvider;
        }

        public void Handle(Domain.Commands.PrendreLapéro prendreLapéro)
        {
            var partieDeChasse = _repository.GetById(prendreLapéro.PartieDeChasseId);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            partieDeChasse.PrendreLapéro(_timeProvider);
            _repository.Save(partieDeChasse);
        }
    }
}