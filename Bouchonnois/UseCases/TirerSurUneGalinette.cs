using Bouchonnois.Domain;
using Bouchonnois.Domain.Exceptions;
using Bouchonnois.UseCases.Exceptions;

namespace Bouchonnois.UseCases
{
    public sealed class TirerSurUneGalinette
    {
        private readonly IPartieDeChasseRepository _repository;
        private readonly Func<DateTime> _timeProvider;

        public TirerSurUneGalinette(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
        {
            _repository = repository;
            _timeProvider = timeProvider;
        }

        public void Handle(Guid id, string chasseur)
        {
            var partieDeChasse = _repository.GetById(id);

            if (partieDeChasse == null)
            {
                throw new LaPartieDeChasseNexistePas();
            }

            if (partieDeChasse.Terrain.NbGalinettes != 0)
            {
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
                                    $"{chasseur} veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main"));
                                _repository.Save(partieDeChasse);

                                throw new TasPlusDeBallesMonVieuxChasseALaMain();
                            }

                            chasseurQuiTire.BallesRestantes--;
                            chasseurQuiTire.NbGalinettes++;
                            partieDeChasse.Terrain.NbGalinettes--;
                            partieDeChasse.Events.Add(new Event(_timeProvider(), $"{chasseur} tire sur une galinette"));
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
            }
            else
            {
                throw new TasTropPicoléMonVieuxTasRienTouché();
            }

            _repository.Save(partieDeChasse);
        }
    }
}