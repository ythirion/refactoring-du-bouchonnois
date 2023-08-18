using Bouchonnois.Domain;
using static Bouchonnois.UseCases.VoidResponse;

namespace Bouchonnois.UseCases
{
    public sealed class ReprendreLaPartie : PartieDeChasseUseCase<Domain.Commands.ReprendreLaPartie, VoidResponse>
    {
        public ReprendreLaPartie(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository, (partieDeChasse, _) => partieDeChasse.Reprendre(timeProvider).Map(_ => Empty))
        {
        }
    }
}