namespace Bouchonnois.Tests.Builders;

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

    public CommandBuilder SurUnTerrainRicheEnGalinettes()
    {
        _nbGalinettes = 3;
        return this;
    }

    public List<(string nom, int nbBalles)> Chasseurs => _chasseurs.ToList();
    public (string nom, int nbGalinettes) Terrain => ("Pitibon sur Sauldre", _nbGalinettes);
}