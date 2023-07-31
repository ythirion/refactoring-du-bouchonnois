using Bouchonnois.Domain;
using Bouchonnois.Domain.Exceptions;

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
            var partieDeChasse = _repository.GetById(id);

            var classement = partieDeChasse
                .Chasseurs
                .GroupBy(c => c.NbGalinettes)
                .OrderByDescending(g => g.Key);

            if (partieDeChasse.Status == PartieStatus.Terminée)
            {
                throw new QuandCestFiniCestFini();
            }

            partieDeChasse.Status = PartieStatus.Terminée;

            string result;

            if (classement.All(group => group.Key == 0))
            {
                result = "Brocouille";
                partieDeChasse.Events.Add(
                    new Event(_timeProvider(), "La partie de chasse est terminée, vainqueur : Brocouille")
                );
            }
            else
            {
                result = string.Join(", ", classement.ElementAt(0).Select(c => c.Nom));
                partieDeChasse.Events.Add(
                    new Event(_timeProvider(),
                        $"La partie de chasse est terminée, vainqueur : {string.Join(", ", classement.ElementAt(0).Select(c => $"{c.Nom} - {c.NbGalinettes} galinettes"))}"
                    )
                );
            }


            _repository.Save(partieDeChasse);

            return result;
        }
    }
}