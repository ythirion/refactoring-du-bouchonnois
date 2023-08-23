using Bouchonnois.Domain.Démarrer;

namespace Bouchonnois.Tests.Builders
{
    public class CommandBuilder
    {
        private (string, int)[] _chasseurs = Array.Empty<(string, int)>();
        private int _nbGalinettes;

        public static CommandBuilder DémarrerUnePartieDeChasse() => new();

        public CommandBuilder Avec(params (string, int)[] chasseurs)
        {
            _chasseurs = chasseurs;
            return this;
        }

        public CommandBuilder SurUnTerrainRicheEnGalinettes(int nbGalinettes = 3)
        {
            _nbGalinettes = nbGalinettes;
            return this;
        }

        public DemarrerPartieDeChasse Build()
            => new(
                new TerrainDeChasse("Pitibon sur Sauldre", _nbGalinettes),
                _chasseurs.Select(c => new Chasseur(c.Item1, c.Item2))
            );
    }
}