namespace Bouchonnois.Domain.Apéro
{
    public record PrendreLapéro(Guid PartieDeChasseId) : PartieDeChasseCommand(PartieDeChasseId);
}