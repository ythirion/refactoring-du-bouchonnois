namespace Bouchonnois.Domain.Commands
{
    public record ConsulterStatus(Guid PartieDeChasseId) : PartieDeChasseCommand(PartieDeChasseId);
}