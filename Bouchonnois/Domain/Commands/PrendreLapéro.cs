namespace Bouchonnois.Domain.Commands
{
    public record PrendreLapéro(Guid PartieDeChasseId) : PartieDeChasseCommand(PartieDeChasseId);
}