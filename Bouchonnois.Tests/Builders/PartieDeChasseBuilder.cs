using Bouchonnois.Domain;
using static Bouchonnois.Domain.PartieStatus;

namespace Bouchonnois.Tests.Builders
{
    public class PartieDeChasseBuilder
    {
        private int _nbGalinettes;
        private ChasseurBuilder[] _chasseurs = {Dédé(), Bernard(), Robert()};
        private PartieStatus _status = EnCours;
        private Event[] _events = Array.Empty<Event>();

        private PartieDeChasseBuilder(int nbGalinettes)
        {
            _nbGalinettes = nbGalinettes;
        }

        public static PartieDeChasseBuilder SurUnTerrainRicheEnGalinettes() => new(3);
        public static PartieDeChasseBuilder SurUnTerrainSansGalinettes() => new(0);

        public static Guid UnePartieDeChasseInexistante() => Guid.NewGuid();

        public PartieDeChasseBuilder Avec(params ChasseurBuilder[] chasseurs)
        {
            _chasseurs = chasseurs;
            return this;
        }

        public PartieDeChasseBuilder ALapéro()
        {
            _status = Apéro;
            return this;
        }

        public PartieDeChasseBuilder Terminée()
        {
            _status = PartieStatus.Terminée;
            return this;
        }

        public PartieDeChasseBuilder Events(params Event[] events)
        {
            _events = events;
            return this;
        }

        public PartieDeChasse Build() =>
            new PartieDeChasse(
                Guid.NewGuid(),
                new Terrain("Pitibon sur Sauldre") {NbGalinettes = _nbGalinettes},
                _chasseurs.Select(c => c.Build()).ToList(),
                _events.ToList(),
                _status
            );
    }
}