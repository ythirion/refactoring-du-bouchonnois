namespace Bouchonnois.Service
{
    public class Terrain
    {
        public Terrain(string nom)
        {
            Nom = nom;
        }

        public string Nom { get; }
        public int NbGalinettes { get; set; }
    }
}