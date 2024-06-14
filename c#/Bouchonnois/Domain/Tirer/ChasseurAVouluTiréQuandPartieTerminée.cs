namespace Bouchonnois.Domain.Tirer
{
    public record ChasseurAVouluTiréQuandPartieTerminée(Guid Id, DateTime Date, string Chasseur)
        : global::Domain.Core.Event(Id, 1, Date)
    {
        public override string ToString() => $"{Chasseur} veut tirer -> On tire pas quand la partie est terminée";
    }
}