using Bouchonnois.Domain;
using static Bouchonnois.Tests.Builders.Functions;

namespace Bouchonnois.Tests.Builders
{
    public class ChasseurBuilder
    {
        private readonly string? _nom;
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

        public ChasseurBuilder Balles(int nbBalles)
        {
            _ballesRestantes = 1;
            return this;
        }

        public static ChasseurBuilder LeChasseur(string nom) => new(nom);
        public static ChasseurBuilder Dédé() => new(Data.Dédé, 20);
        public static ChasseurBuilder Bernard() => new(Data.Bernard, 8);
        public static ChasseurBuilder Robert() => new(Data.Robert, 12);

        public Chasseur Build()
        {
            var chasseur = new Chasseur(_nom!, _ballesRestantes);
            Repeat(_nbGalinettes, () => chasseur.ATué());
            return chasseur;
        }
    }
}