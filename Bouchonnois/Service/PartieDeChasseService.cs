using Bouchonnois.Domain;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.UseCases;

namespace Bouchonnois.Service
{
    public class PartieDeChasseService
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly DemarrerPartieDeChasse _demarrerPartieDeChasse;
        private readonly TirerSurUneGalinette _tirerSurUneGalinette;
        private readonly Tirer _tirer;
        private readonly PrendreLapéro _prendreLapéro;
        private readonly ReprendreLaPartie _reprendreLaPartie;
        private readonly TerminerLaPartie _terminerLaPartie;

        public PartieDeChasseService(
            IPartieDeChasseRepository repository,
            Func<DateTime> timeProvider)
        {
            _repository = repository;
            _terminerLaPartie = new TerminerLaPartie(repository, timeProvider);
            _reprendreLaPartie = new ReprendreLaPartie(repository, timeProvider);
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

        public void ReprendreLaPartie(Guid id) => _reprendreLaPartie.Handle(id);

        public string TerminerLaPartie(Guid id) => _terminerLaPartie.Handle(id);

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