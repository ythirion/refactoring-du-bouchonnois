using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class ReprendreLaPartie : PartieDeChasseUseCase<Domain.Reprendre.ReprendreLaPartie, VoidResponse>
    {
        public ReprendreLaPartie(IPartieDeChasseRepository repository)
            : base(repository, (partieDeChasse, _) => ToEmpty(partieDeChasse.Reprendre()))
        {
        }
    }
}