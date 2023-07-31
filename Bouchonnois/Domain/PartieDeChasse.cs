using System.Collections.Immutable;
using Bouchonnois.Domain.Exceptions;
using static System.String;
using static Bouchonnois.Domain.PartieStatus;

namespace Bouchonnois.Domain
{
    public sealed class PartieDeChasse
    {
        private readonly List<Chasseur> _chasseurs;
        private readonly List<Event> _events;

        public Guid Id { get; }
        public IReadOnlyList<Chasseur> Chasseurs => _chasseurs.ToImmutableArray();
        public Terrain Terrain { get; }
        public PartieStatus Status { get; private set; }
        public IReadOnlyList<Event> Events => _events.ToImmutableArray();


        private PartieDeChasse(Guid id,
            Func<DateTime> timeProvider,
            Terrain terrain,
            List<(string nom, int nbBalles)> chasseurs)
        {
            Id = id;
            _chasseurs = new List<Chasseur>();
            Terrain = terrain;
            Status = EnCours;
            _events = new List<Event>();

            foreach (var chasseur in chasseurs)
            {
                _chasseurs.Add(new Chasseur(chasseur.nom)
                {
                    BallesRestantes = chasseur.nbBalles
                });
            }

            string chasseursToString = Join(
                ", ",
                _chasseurs.Select(c => c.Nom + $" ({c.BallesRestantes} balles)")
            );

            _events.Add(new Event(timeProvider(),
                $"La partie de chasse commence à {Terrain.Nom} avec {chasseursToString}")
            );
        }

        public static PartieDeChasse Create(
            Func<DateTime> timeProvider,
            (string nom, int nbGalinettes) terrainDeChasse,
            List<(string nom, int nbBalles)> chasseurs)
        {
            if (terrainDeChasse.nbGalinettes <= 0)
            {
                throw new ImpossibleDeDémarrerUnePartieSansGalinettes();
            }

            if (chasseurs.Count == 0)
            {
                throw new ImpossibleDeDémarrerUnePartieSansChasseur();
            }

            if (chasseurs.Any(c => c.nbBalles == 0))
            {
                throw new ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle();
            }

            return new PartieDeChasse(Guid.NewGuid(),
                timeProvider,
                new Terrain(terrainDeChasse.nom)
                {
                    NbGalinettes = terrainDeChasse.nbGalinettes
                },
                chasseurs
            );
        }

        public void PrendreLapéro(Func<DateTime> timeProvider)
        {
            if (Status == Apéro)
            {
                throw new OnEstDéjàEnTrainDePrendreLapéro();
            }
            else if (Status == Terminée)
            {
                throw new OnPrendPasLapéroQuandLaPartieEstTerminée();
            }

            Status = Apéro;
            _events.Add(new Event(timeProvider(), "Petit apéro"));
        }

        public void Reprendre(Func<DateTime> timeProvider)
        {
            if (Status == EnCours)
            {
                throw new LaChasseEstDéjàEnCours();
            }

            if (Status == Terminée)
            {
                throw new QuandCestFiniCestFini();
            }

            Status = EnCours;
            _events.Add(new Event(timeProvider(), "Reprise de la chasse"));
        }

        public string Consulter() =>
            Join(
                Environment.NewLine,
                Events
                    .OrderByDescending(@event => @event.Date)
                    .Select(@event => @event.ToString())
            );

        public string Terminer(Func<DateTime> timeProvider)
        {
            var classement = Chasseurs
                .GroupBy(c => c.NbGalinettes)
                .OrderByDescending(g => g.Key);

            if (Status == Terminée)
            {
                throw new QuandCestFiniCestFini();
            }

            Status = Terminée;

            string result;

            if (classement.All(group => group.Key == 0))
            {
                result = "Brocouille";
                _events.Add(
                    new Event(timeProvider(), "La partie de chasse est terminée, vainqueur : Brocouille")
                );
            }
            else
            {
                result = Join(", ", classement.ElementAt(0).Select(c => c.Nom));
                _events.Add(
                    new Event(timeProvider(),
                        $"La partie de chasse est terminée, vainqueur : {Join(", ", classement.ElementAt(0).Select(c => $"{c.Nom} - {c.NbGalinettes} galinettes"))}"
                    )
                );
            }

            return result;
        }

        public void Tirer(string chasseur, Func<DateTime> timeProvider,
            IPartieDeChasseRepository repository)
        {
            if (Status != Apéro)
            {
                if (Status != Terminée)
                {
                    if (_chasseurs.Exists(c => c.Nom == chasseur))
                    {
                        var chasseurQuiTire = _chasseurs.Find(c => c.Nom == chasseur)!;

                        if (chasseurQuiTire.BallesRestantes == 0)
                        {
                            _events.Add(new Event(timeProvider(),
                                $"{chasseur} tire -> T'as plus de balles mon vieux, chasse à la main"));
                            repository.Save(this);

                            throw new TasPlusDeBallesMonVieuxChasseALaMain();
                        }

                        _events.Add(new Event(timeProvider(), $"{chasseur} tire"));
                        chasseurQuiTire.BallesRestantes--;
                    }
                    else
                    {
                        throw new ChasseurInconnu(chasseur);
                    }
                }
                else
                {
                    _events.Add(new Event(timeProvider(),
                        $"{chasseur} veut tirer -> On tire pas quand la partie est terminée"));
                    repository.Save(this);

                    throw new OnTirePasQuandLaPartieEstTerminée();
                }
            }
            else
            {
                _events.Add(new Event(timeProvider(),
                    $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));
                repository.Save(this);

                throw new OnTirePasPendantLapéroCestSacré();
            }
        }

        public void TirerSurUneGalinette(string chasseur,
            Func<DateTime> timeProvider,
            IPartieDeChasseRepository repository)
        {
            if (Terrain.NbGalinettes != 0)
            {
                if (Status != Apéro)
                {
                    if (Status != Terminée)
                    {
                        if (_chasseurs.Exists(c => c.Nom == chasseur))
                        {
                            var chasseurQuiTire = _chasseurs.Find(c => c.Nom == chasseur)!;

                            if (chasseurQuiTire.BallesRestantes == 0)
                            {
                                _events.Add(new Event(timeProvider(),
                                    $"{chasseur} veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main"));
                                repository.Save(this);

                                throw new TasPlusDeBallesMonVieuxChasseALaMain();
                            }

                            chasseurQuiTire.BallesRestantes--;
                            chasseurQuiTire.NbGalinettes++;
                            Terrain.NbGalinettes--;
                            _events.Add(new Event(timeProvider(), $"{chasseur} tire sur une galinette"));
                        }
                        else
                        {
                            throw new ChasseurInconnu(chasseur);
                        }
                    }
                    else
                    {
                        _events.Add(new Event(timeProvider(),
                            $"{chasseur} veut tirer -> On tire pas quand la partie est terminée"));
                        repository.Save(this);

                        throw new OnTirePasQuandLaPartieEstTerminée();
                    }
                }
                else
                {
                    _events.Add(new Event(timeProvider(),
                        $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));
                    repository.Save(this);
                    throw new OnTirePasPendantLapéroCestSacré();
                }
            }
            else
            {
                throw new TasTropPicoléMonVieuxTasRienTouché();
            }
        }
    }
}