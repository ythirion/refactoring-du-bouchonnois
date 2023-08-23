namespace Bouchonnois.Domain.Tirer
{
    public record ChasseurATiréSurUneGalinette(Guid Id, DateTime Date, string Chasseur) : global::Domain.Core.Event(Id,
        1, Date)
    {
        public override string ToString() => $"{Chasseur} tire sur une galinette";
    }
}