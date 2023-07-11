using Bouchonnois.Domain;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.UseCases;

namespace Bouchonnois.Service
{
    public class PartieDeChasseService
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly Func<DateTime> _timeProvider;
        private readonly DemarrerPartieDeChasse _demarrerPartieDeChasse;
        private readonly TirerSurUneGalinette _tirerSurUneGalinette;
        private readonly Tirer _tirer;
        private readonly PrendreLapéro _prendreLapéro;

        public PartieDeChasseService(
            IPartieDeChasseRepository repository,
            Func<DateTime> timeProvider)
        {
            _repository = repository;
            _timeProvider = timeProvider;
            _prendreLapéro = new PrendreLapéro(repository, timeProvider);
            _tirer = new Tirer(repository, timeProvider);
            _tirerSurUneGalinette = new TirerSurUneGalinette(repository, timeProvider);
            _demarrerPartieDeChasse = new DemarrerPartieDeChasse(repository, timeProvider);
        }

        public Guid Demarrer((string nom, int nbGalinettes) terrainDeChasse, List<(string nom, int nbBalles)> chasseurs)
            => _demarrerPartieDeChasse.Handle(terrainDeChasse, chasseurs);

        public void TirerSurUneGalinette(Guid id, string chasseur)
            => _tirerSurUneGalinette.Handle(id, chasseur);

        public void Tirer(Guid id, string chasseur) => _tirer.Handle(id, chasseur);

        public void PrendreLapéro(Guid id) => _prendreLapéro.Handle(id);

        public void ReprendreLaPartie(Guid id)
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

        public string TerminerLaPartie(Guid id)
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

        public string ConsulterStatus(Guid id)
        {
            var partieDeChasse = _repository.GetById(id);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            return string.Join(
                Environment.NewLine,
                partieDeChasse.Events
                    .OrderByDescending(@event => @event.Date)
                    .Select(@event => @event.ToString())
            );
        }
    }
}