namespace Domain.Core
{
    public abstract record Event(Guid Id, int Version, DateTime Date) : IEvent;
}