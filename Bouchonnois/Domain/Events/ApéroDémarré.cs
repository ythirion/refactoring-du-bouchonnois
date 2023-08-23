namespace Bouchonnois.Domain.Events
{
    public sealed record ApéroDémarré(Guid Id, DateTime Date) : global::Domain.Core.Event(Id, 1, Date);
}