namespace Bouchonnois.Domain.Tirer
{
    public record TirerSurUneGalinette
        (Guid PartieDeChasseId, string Chasseur) : PartieDeChasseCommand(PartieDeChasseId);
}