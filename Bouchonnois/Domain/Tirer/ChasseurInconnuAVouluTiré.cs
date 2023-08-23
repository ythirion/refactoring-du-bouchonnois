namespace Bouchonnois.Domain.Tirer
{
    public record ChasseurInconnuAVouluTiré(Guid Id, DateTime Date, string Chasseur)
        : global::Domain.Core.Event(Id, 1, Date)
    {
        public override string ToString() => $"Chasseur inconnu {Chasseur}";
    }
}