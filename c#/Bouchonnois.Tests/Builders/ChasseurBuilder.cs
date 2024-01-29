using Bouchonnois.Domain;

namespace Bouchonnois.Tests.Builders
{
    public class ChasseurBuilder
    {
        private string? _nom;
        private int _ballesRestantes;
        private int _nbGalinettes;

        public ChasseurBuilder(string nom) => _nom = nom;

        private ChasseurBuilder(string nom, int ballesRestantes)
        {
            _nom = nom;
            _ballesRestantes = ballesRestantes;
        }

        public ChasseurBuilder SansBalles()
        {
            _ballesRestantes = 0;
            return this;
        }

        public ChasseurBuilder AyantTué(int nbGalinettes)
        {
            _nbGalinettes = nbGalinettes;
            return this;
        }

        public static ChasseurBuilder LeChasseur(string nom) => new(nom);
        public static ChasseurBuilder Dédé() => new ChasseurBuilder("Dédé", 20);
        public static ChasseurBuilder Bernard() => new ChasseurBuilder("Bernard", 8);
        public static ChasseurBuilder Robert() => new ChasseurBuilder("Robert", 12);

        public Chasseur Build() => new(_nom!) {BallesRestantes = _ballesRestantes, NbGalinettes = _nbGalinettes};
    }
}