using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class ReprendreLaPartie : PartieDeChasseUseCase<Domain.Commands.ReprendreLaPartie, VoidResponse>
    {
        public ReprendreLaPartie(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository, (partieDeChasse, _) => ToEmpty(partieDeChasse.Reprendre(timeProvider)))
        {
        }
    }
}