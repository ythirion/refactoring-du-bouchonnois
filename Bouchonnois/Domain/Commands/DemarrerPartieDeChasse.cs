namespace Bouchonnois.Domain.Commands
{
    public record DemarrerPartieDeChasse(TerrainDeChasse TerrainDeChasse, IEnumerable<Chasseur> Chasseurs) : ICommand;

    public record TerrainDeChasse(string Nom, int NbGalinettes);

    public record Chasseur(string Nom, int NbBalles);
}