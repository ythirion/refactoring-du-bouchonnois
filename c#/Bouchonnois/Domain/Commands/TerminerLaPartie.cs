namespace Bouchonnois.Domain.Commands
{
    public record TerminerLaPartie(Guid PartieDeChasseId) : PartieDeChasseCommand(PartieDeChasseId);
}