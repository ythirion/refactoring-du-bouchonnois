namespace Bouchonnois.Domain.Terminer
{
    public record TerminerLaPartie(Guid PartieDeChasseId) : PartieDeChasseCommand(PartieDeChasseId);
}