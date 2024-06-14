namespace Bouchonnois.Domain.Tirer
{
    public record Tirer(Guid PartieDeChasseId, string Chasseur) : PartieDeChasseCommand(PartieDeChasseId);
}