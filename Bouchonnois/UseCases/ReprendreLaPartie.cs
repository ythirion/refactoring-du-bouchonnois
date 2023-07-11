using Bouchonnois.Domain;
using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.UseCases
{
    public class ReprendreLaPartie
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly Func<DateTime> _timeProvider;

        public ReprendreLaPartie(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
        {
            _repository = repository;
            _timeProvider = timeProvider;
        }

        public void Handle(Guid id)
        {
            var partieDeChasse = _repository.GetById(id);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            if (partieDeChasse.Status == PartieStatus.EnCours)
            {
                throw new LaChasseEstDéjàEnCours();
            }

            if (partieDeChasse.Status == PartieStatus.Terminée)
            {
                throw new QuandCestFiniCestFini();
            }

            partieDeChasse.Status = PartieStatus.EnCours;
            partieDeChasse.Events.Add(new Event(_timeProvider(), "Reprise de la chasse"));
            _repository.Save(partieDeChasse);
        }
    }
}