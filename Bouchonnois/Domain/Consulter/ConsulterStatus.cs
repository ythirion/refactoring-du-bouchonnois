namespace Bouchonnois.Domain.Consulter
{
    public record ConsulterStatus(Guid PartieDeChasseId) : PartieDeChasseCommand(PartieDeChasseId);
}