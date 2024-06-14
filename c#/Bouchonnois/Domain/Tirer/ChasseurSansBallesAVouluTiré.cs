namespace Bouchonnois.Domain.Tirer
{
    public record ChasseurSansBallesAVouluTiré(Guid Id, DateTime Date, string Chasseur, string SurQuoi)
        : global::Domain.Core.Event(Id, 1, Date)
    {
        public override string ToString() => $"{Chasseur} {SurQuoi} -> T'as plus de balles mon vieux, chasse à la main";
    }
}