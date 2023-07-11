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

        public PartieDeChasseService(
            IPartieDeChasseRepository repository,
            Func<DateTime> timeProvider)
        {
            _repository = repository;
            _timeProvider = timeProvider;

            _tirerSurUneGalinette = new TirerSurUneGalinette(repository, timeProvider);
            _demarrerPartieDeChasse = new DemarrerPartieDeChasse(repository, timeProvider);
        }

        public Guid Demarrer((string nom, int nbGalinettes) terrainDeChasse, List<(string nom, int nbBalles)> chasseurs)
            => _demarrerPartieDeChasse.Handle(terrainDeChasse, chasseurs);

        public void TirerSurUneGalinette(Guid id, string chasseur)
            => _tirerSurUneGalinette.Handle(id, chasseur);

        public void Tirer(Guid id, string chasseur)
        {
            var partieDeChasse = _repository.GetById(id);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            if (partieDeChasse.Status != PartieStatus.Apéro)
            {
                if (partieDeChasse.Status != PartieStatus.Terminée)
                {
                    if (partieDeChasse.Chasseurs.Exists(c => c.Nom == chasseur))
                    {
                        var chasseurQuiTire = partieDeChasse.Chasseurs.Find(c => c.Nom == chasseur)!;

                        if (chasseurQuiTire.BallesRestantes == 0)
                        {
                            partieDeChasse.Events.Add(new Event(_timeProvider(),
                                $"{chasseur} tire -> T'as plus de balles mon vieux, chasse à la main"));
                            _repository.Save(partieDeChasse);

                            throw new TasPlusDeBallesMonVieuxChasseALaMain();
                        }

                        partieDeChasse.Events.Add(new Event(_timeProvider(), $"{chasseur} tire"));
                        chasseurQuiTire.BallesRestantes--;
                    }
                    else
                    {
                        throw new ChasseurInconnu(chasseur);
                    }
                }
                else
                {
                    partieDeChasse.Events.Add(new Event(_timeProvider(),
                        $"{chasseur} veut tirer -> On tire pas quand la partie est terminée"));
                    _repository.Save(partieDeChasse);

                    throw new OnTirePasQuandLaPartieEstTerminée();
                }
            }
            else
            {
                partieDeChasse.Events.Add(new Event(_timeProvider(),
                    $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));
                _repository.Save(partieDeChasse);

                throw new OnTirePasPendantLapéroCestSacré();
            }

            _repository.Save(partieDeChasse);
        }

        public void PrendreLapéro(Guid id)
        {
            var partieDeChasse = _repository.GetById(id);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            if (partieDeChasse.Status == PartieStatus.Apéro)
            {
                throw new OnEstDéjàEnTrainDePrendreLapéro();
            }
            else if (partieDeChasse.Status == PartieStatus.Terminée)
            {
                throw new OnPrendPasLapéroQuandLaPartieEstTerminée();
            }
            else
            {
                partieDeChasse.Status = PartieStatus.Apéro;
                partieDeChasse.Events.Add(new Event(_timeProvider(), "Petit apéro"));
                _repository.Save(partieDeChasse);
            }
        }

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