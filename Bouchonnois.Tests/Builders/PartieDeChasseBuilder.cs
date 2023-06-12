using Bouchonnois.Domain;
using Bouchonnois.Service;
using static Bouchonnois.Domain.PartieStatus;

namespace Bouchonnois.Tests.Builders
{
    public class PartieDeChasseBuilder
    {
        private int _nbGalinettes;
        private ChasseurBuilder[] _chasseurs = {Dédé(), Bernard(), Robert()};
        private PartieStatus _status = EnCours;
        private Event[] _events = Array.Empty<Event>();

        public static PartieDeChasseBuilder UnePartieDeChasseDuBouchonnois() => new();
        public static Guid UnePartieDeChasseInexistante() => Guid.NewGuid();

        public PartieDeChasseBuilder SurUnTerrainRicheEnGalinettes(int nbGalinettes = 3)
        {
            _nbGalinettes = nbGalinettes;
            return this;
        }

        public PartieDeChasseBuilder SurUnTerrainSansGalinettes()
        {
            _nbGalinettes = 0;
            return this;
        }

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

        public PartieDeChasse Build() => new(
            Guid.NewGuid(),
            new Terrain("Pitibon sur Sauldre") {NbGalinettes = _nbGalinettes},
            _chasseurs.Select(c => c.Build()).ToList(),
            _events.ToList(),
            _status
        );
    }
}