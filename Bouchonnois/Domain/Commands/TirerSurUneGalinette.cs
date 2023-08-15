namespace Bouchonnois.Domain.Commands
{
    public record TirerSurUneGalinette
        (Guid PartieDeChasseId, string Chasseur) : PartieDeChasseCommand(PartieDeChasseId);
}