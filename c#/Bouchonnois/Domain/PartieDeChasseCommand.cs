namespace Bouchonnois.Domain
{
    public record PartieDeChasseCommand(Guid PartieDeChasseId) : ICommand;
}