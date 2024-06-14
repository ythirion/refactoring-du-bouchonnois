namespace Bouchonnois.Domain.Tirer
{
    public record ChasseurACruTiréSurGalinette(Guid Id, DateTime Date, string Chasseur) : global::Domain.Core.Event(Id,
        1, Date)
    {
        public override string ToString() => $"{Chasseur}, t'as trop picolé mon vieux, t'as rien touché";
    }
}