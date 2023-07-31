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

        #region Create

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

            EmitPartieDémarrée(timeProvider);
        }

        public static PartieDeChasse Create(
            Func<DateTime> timeProvider,
            (string nom, int nbGalinettes) terrainDeChasse,
            List<(string nom, int nbBalles)> chasseurs)
        {
            CheckTerrainValide(terrainDeChasse);
            CheckChasseursValides(chasseurs);

            return new PartieDeChasse(
                Guid.NewGuid(),
                timeProvider,
                new Terrain(terrainDeChasse.nom)
                {
                    NbGalinettes = terrainDeChasse.nbGalinettes
                },
                chasseurs
            );
        }

        private static void CheckTerrainValide((string nom, int nbGalinettes) terrainDeChasse)
        {
            if (terrainDeChasse.nbGalinettes <= 0)
            {
                throw new ImpossibleDeDémarrerUnePartieSansGalinettes();
            }
        }

        private static void CheckChasseursValides(List<(string nom, int nbBalles)> chasseurs)
        {
            if (chasseurs.Count == 0)
            {
                throw new ImpossibleDeDémarrerUnePartieSansChasseur();
            }

            if (AuMoinsUnChasseurNAPasDeBalles(chasseurs))
            {
                throw new ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle();
            }
        }

        private static bool AuMoinsUnChasseurNAPasDeBalles(IEnumerable<(string nom, int nbBalles)> chasseurs)
            => chasseurs.Any(c => c.nbBalles == 0);

        private void EmitPartieDémarrée(Func<DateTime> timeProvider)
        {
            var chasseursToString = Join(", ", _chasseurs.Select(c => c.Nom + $" ({c.BallesRestantes} balles)"));
            EmitEvent(timeProvider, $"La partie de chasse commence à {Terrain.Nom} avec {chasseursToString}");
        }

        #endregion


        #region Apéro

        public void PrendreLapéro(Func<DateTime> timeProvider)
        {
            if (Status == Apéro)
            {
                throw new OnEstDéjàEnTrainDePrendreLapéro();
            }

            if (Status == Terminée)
            {
                throw new OnPrendPasLapéroQuandLaPartieEstTerminée();
            }

            Status = Apéro;
            EmitEvent(timeProvider, "Petit apéro");
        }

        #endregion

        #region Reprendre

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
            EmitEvent(timeProvider, "Reprise de la chasse");
        }

        #endregion

        #region Consulter

        public string Consulter() =>
            Join(
                Environment.NewLine,
                Events
                    .OrderByDescending(@event => @event.Date)
                    .Select(@event => @event.ToString())
            );

        #endregion

        #region Terminer

        public string Terminer(Func<DateTime> timeProvider)
        {
            if (Status == Terminée)
            {
                throw new QuandCestFiniCestFini();
            }

            Status = Terminée;
            string result;

            var classement = Classement();

            if (TousBrocouilles(classement))
            {
                result = "Brocouille";
                EmitEvent(timeProvider, "La partie de chasse est terminée, vainqueur : Brocouille");
            }
            else
            {
                result = Join(", ", classement.ElementAt(0).Select(c => c.Nom));
                EmitEvent(timeProvider,
                    $"La partie de chasse est terminée, vainqueur : {Join(", ", classement.ElementAt(0).Select(c => $"{c.Nom} - {c.NbGalinettes} galinettes"))}");
            }

            return result;
        }

        private IOrderedEnumerable<IGrouping<int, Chasseur>> Classement()
            => Chasseurs
                .GroupBy(c => c.NbGalinettes)
                .OrderByDescending(g => g.Key);

        private static bool TousBrocouilles(IOrderedEnumerable<IGrouping<int, Chasseur>> classement) =>
            classement.All(group => group.Key == 0);

        #endregion

        #region Terminer

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

        #endregion


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

        private void EmitEvent(Func<DateTime> timeProvider, string message) =>
            _events.Add(new Event(timeProvider(), message));
    }
}