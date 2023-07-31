namespace Bouchonnois.Domain
{
    public sealed class Terrain
    {
        public Terrain(string nom, int nbGalinettes)
        {
            Nom = nom;
            NbGalinettes = nbGalinettes;
        }

        public string Nom { get; }
        public int NbGalinettes { get; private set; }

        public void UneGalinetteEnMoins() => NbGalinettes--;
    }
}