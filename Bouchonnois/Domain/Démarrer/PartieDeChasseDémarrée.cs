namespace Bouchonnois.Domain.Démarrer
{
    public record TerrainCréé(string Nom, int NbGalinettes);

    public record ChasseurCréé(string Nom, int BallesRestantes);

    public record PartieDeChasseDémarrée(Guid Id, DateTime Date, TerrainCréé Terrain, ChasseurCréé[] Chasseurs)
        : global::Domain.Core.Event(Id, 1, Date)
    {
        public override string ToString()
            => $"La partie de chasse commence à {Terrain?.Nom} avec {
                string.Join(", ", Chasseurs.Map(c => c.Nom + $" ({c.BallesRestantes} balles)"))
            }";
    }
}