namespace Bouchonnois.Domain
{
    public sealed class Chasseur
    {
        public Chasseur(string nom, int ballesRestantes)
        {
            Nom = nom;
            BallesRestantes = ballesRestantes;
        }

        public bool AEncoreDesBalles() => BallesRestantes > 0;

        public string Nom { get; }
        public int BallesRestantes { get; private set; }
        public int NbGalinettes { get; private set; }

        public void ATiré() => BallesRestantes--;

        public void ATué() => NbGalinettes++;
    }
}