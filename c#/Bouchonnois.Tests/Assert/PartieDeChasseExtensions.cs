using Bouchonnois.Domain;

namespace Bouchonnois.Tests.Assert
{
    public static class PartieDeChasseExtensions
    {
        public static PartieDeChasseAssertions Should(this PartieDeChasse? partieDeChasse) => new(partieDeChasse);
    }
}