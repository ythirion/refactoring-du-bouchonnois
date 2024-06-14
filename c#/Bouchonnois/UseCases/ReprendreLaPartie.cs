using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class ReprendreLaPartie : EmptyResponsePartieDeChasseUseCase<Domain.Commands.ReprendreLaPartie>
    {
        public ReprendreLaPartie(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository, (partieDeChasse, _) => partieDeChasse.Reprendre(timeProvider))
        {
        }
    }
}