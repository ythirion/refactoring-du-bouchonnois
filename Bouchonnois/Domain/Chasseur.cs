namespace Bouchonnois.Domain
{
    public sealed class Chasseur
    {
        public Chasseur(string nom)
        {
            Nom = nom;
        }

        public bool AEncoreDesBalles() => BallesRestantes > 0;

        public string Nom { get; }
        public int BallesRestantes { get; set; }
        public int NbGalinettes { get; set; }
    }
}