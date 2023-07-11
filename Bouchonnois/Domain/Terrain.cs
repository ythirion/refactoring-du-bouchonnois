namespace Bouchonnois.Domain
{
    public sealed class Terrain
    {
        public Terrain(string nom)
        {
            Nom = nom;
        }

        public string Nom { get; }
        public int NbGalinettes { get; set; }
    }
}