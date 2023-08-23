namespace Bouchonnois.Domain.Reprendre
{
    public record PartieReprise(Guid Id, DateTime Date) : global::Domain.Core.Event(Id, 1, Date)
    {
        public override string ToString() => "Reprise de la chasse";
    }
}