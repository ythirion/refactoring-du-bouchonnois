namespace Bouchonnois.Domain
{
    public class Chasseur
    {
        public Chasseur(string nom)
        {
            Nom = nom;
        }

        public string Nom { get; }
        public int BallesRestantes { get; set; }
        public int NbGalinettes { get; set; }
    }
}