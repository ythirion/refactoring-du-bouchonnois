namespace Bouchonnois.Domain.Apéro
{
    public sealed record ApéroDémarré(Guid Id, DateTime Date) : global::Domain.Core.Event(Id, 1, Date)
    {
        public override string ToString() => "Petit apéro";
    }
}