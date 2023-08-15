namespace Bouchonnois.Domain.Commands
{
    public record PartieDeChasseCommand(Guid PartieDeChasseId) : ICommand;
}