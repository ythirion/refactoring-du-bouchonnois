namespace Bouchonnois.Domain.Reprendre
{
    public record ReprendreLaPartie(Guid PartieDeChasseId) : PartieDeChasseCommand(PartieDeChasseId);
}