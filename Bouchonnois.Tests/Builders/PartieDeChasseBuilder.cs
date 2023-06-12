using Bouchonnois.Domain;
using Bouchonnois.Service;

namespace Bouchonnois.Tests.Builders
{
    public class PartieDeChasseBuilder
    {
        private int _nbGalinettes;
        private ChasseurBuilder[] _chasseurs = Array.Empty<ChasseurBuilder>();

        public static PartieDeChasseBuilder UnePartieDeChasseDuBouchonnois() => new();

        public PartieDeChasseBuilder SurUnTerrainRicheEnGalinettes(int nbGalinettes = 3)
        {
            _nbGalinettes = nbGalinettes;
            return this;
        }

        public PartieDeChasseBuilder Avec(params ChasseurBuilder[] chasseurs)
        {
            _chasseurs = chasseurs;
            return this;
        }

        public PartieDeChasse Build() => new(
            Guid.NewGuid(),
            new Terrain("Pitibon sur Sauldre") {NbGalinettes = _nbGalinettes},
            _chasseurs.Select(c => c.Build()).ToList()
        );
    }
}