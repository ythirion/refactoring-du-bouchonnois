namespace Bouchonnois.Domain.Exceptions
{
    public class ChasseurInconnu : Exception
    {
        public ChasseurInconnu(string chasseur)
            : base($"Chasseur inconnu {chasseur}")
        {
        }
    }
}