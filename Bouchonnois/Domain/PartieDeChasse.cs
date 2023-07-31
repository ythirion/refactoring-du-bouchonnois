using Bouchonnois.Domain.Exceptions;
using static System.String;
using static Bouchonnois.Domain.PartieStatus;

namespace Bouchonnois.Domain
{
    public sealed class PartieDeChasse
    {
        private PartieDeChasse(Guid id, Terrain terrain)
        {
            Id = id;
            Chasseurs = new List<Chasseur>();
            Terrain = terrain;
            Status = EnCours;
            Events = new List<Event>();
        }

        public Guid Id { get; }
        public List<Chasseur> Chasseurs { get; }
        public Terrain Terrain { get; }
        public PartieStatus Status { get; private set; }
        public List<Event> Events { get; set; }

        public static PartieDeChasse Create(
            Func<DateTime> timeProvider,
            (string nom, int nbGalinettes) terrainDeChasse,
            List<(string nom, int nbBalles)> chasseurs)
        {
            if (terrainDeChasse.nbGalinettes <= 0)
            {
                throw new ImpossibleDeDémarrerUnePartieSansGalinettes();
            }

            var partieDeChasse =
                new PartieDeChasse(Guid.NewGuid(),
                    new Terrain(terrainDeChasse.nom)
                    {
                        NbGalinettes = terrainDeChasse.nbGalinettes
                    }
                );

            foreach (var chasseur in chasseurs)
            {
                if (chasseur.nbBalles == 0)
                {
                    throw new ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle();
                }

                partieDeChasse.Chasseurs.Add(new Chasseur(chasseur.nom)
                {
                    BallesRestantes = chasseur.nbBalles
                });
            }

            if (partieDeChasse.Chasseurs.Count == 0)
            {
                throw new ImpossibleDeDémarrerUnePartieSansChasseur();
            }

            string chasseursToString = string.Join(
                ", ",
                partieDeChasse.Chasseurs.Select(c => c.Nom + $" ({c.BallesRestantes} balles)")
            );

            partieDeChasse.Events.Add(new Event(timeProvider(),
                $"La partie de chasse commence à {partieDeChasse.Terrain.Nom} avec {chasseursToString}")
            );

            return partieDeChasse;
        }

        public void PrendreLapéro(Func<DateTime> timeProvider)
        {
            if (Status == PartieStatus.Apéro)
            {
                throw new OnEstDéjàEnTrainDePrendreLapéro();
            }
            else if (Status == PartieStatus.Terminée)
            {
                throw new OnPrendPasLapéroQuandLaPartieEstTerminée();
            }

            Status = PartieStatus.Apéro;
            Events.Add(new Event(timeProvider(), "Petit apéro"));
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
            Events.Add(new Event(timeProvider(), "Reprise de la chasse"));
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
            var classement = this
                .Chasseurs
                .GroupBy(c => c.NbGalinettes)
                .OrderByDescending(g => g.Key);

            if (this.Status == PartieStatus.Terminée)
            {
                throw new QuandCestFiniCestFini();
            }

            this.Status = PartieStatus.Terminée;

            string result;

            if (classement.All(group => group.Key == 0))
            {
                result = "Brocouille";
                this.Events.Add(
                    new Event(timeProvider(), "La partie de chasse est terminée, vainqueur : Brocouille")
                );
            }
            else
            {
                result = string.Join(", ", classement.ElementAt(0).Select(c => c.Nom));
                this.Events.Add(
                    new Event(timeProvider(),
                        $"La partie de chasse est terminée, vainqueur : {string.Join(", ", classement.ElementAt(0).Select(c => $"{c.Nom} - {c.NbGalinettes} galinettes"))}"
                    )
                );
            }

            return result;
        }

        public void Tirer(string chasseur, Func<DateTime> timeProvider,
            IPartieDeChasseRepository repository)
        {
            if (this.Status != PartieStatus.Apéro)
            {
                if (this.Status != PartieStatus.Terminée)
                {
                    if (this.Chasseurs.Exists(c => c.Nom == chasseur))
                    {
                        var chasseurQuiTire = this.Chasseurs.Find(c => c.Nom == chasseur)!;

                        if (chasseurQuiTire.BallesRestantes == 0)
                        {
                            this.Events.Add(new Event(timeProvider(),
                                $"{chasseur} tire -> T'as plus de balles mon vieux, chasse à la main"));
                            repository.Save(this);

                            throw new TasPlusDeBallesMonVieuxChasseALaMain();
                        }

                        this.Events.Add(new Event(timeProvider(), $"{chasseur} tire"));
                        chasseurQuiTire.BallesRestantes--;
                    }
                    else
                    {
                        throw new ChasseurInconnu(chasseur);
                    }
                }
                else
                {
                    this.Events.Add(new Event(timeProvider(),
                        $"{chasseur} veut tirer -> On tire pas quand la partie est terminée"));
                    repository.Save(this);

                    throw new OnTirePasQuandLaPartieEstTerminée();
                }
            }
            else
            {
                this.Events.Add(new Event(timeProvider(),
                    $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));
                repository.Save(this);

                throw new OnTirePasPendantLapéroCestSacré();
            }
        }

        public void TirerSurUneGalinette(string chasseur,
            Func<DateTime> timeProvider,
            IPartieDeChasseRepository repository)
        {
            if (this.Terrain.NbGalinettes != 0)
            {
                if (this.Status != PartieStatus.Apéro)
                {
                    if (this.Status != PartieStatus.Terminée)
                    {
                        if (this.Chasseurs.Exists(c => c.Nom == chasseur))
                        {
                            var chasseurQuiTire = this.Chasseurs.Find(c => c.Nom == chasseur)!;

                            if (chasseurQuiTire.BallesRestantes == 0)
                            {
                                this.Events.Add(new Event(timeProvider(),
                                    $"{chasseur} veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main"));
                                repository.Save(this);

                                throw new TasPlusDeBallesMonVieuxChasseALaMain();
                            }

                            chasseurQuiTire.BallesRestantes--;
                            chasseurQuiTire.NbGalinettes++;
                            this.Terrain.NbGalinettes--;
                            this.Events.Add(new Event(timeProvider(), $"{chasseur} tire sur une galinette"));
                        }
                        else
                        {
                            throw new ChasseurInconnu(chasseur);
                        }
                    }
                    else
                    {
                        this.Events.Add(new Event(timeProvider(),
                            $"{chasseur} veut tirer -> On tire pas quand la partie est terminée"));
                        repository.Save(this);

                        throw new OnTirePasQuandLaPartieEstTerminée();
                    }
                }
                else
                {
                    this.Events.Add(new Event(timeProvider(),
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