using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class PrendreLapéro : PartieDeChasseUseCase<Domain.Apéro.PrendreLapéro, VoidResponse>
    {
        public PrendreLapéro(IPartieDeChasseRepository repository)
            : base(repository,
                (partieDeChasse, _) => ToEmpty(partieDeChasse.PrendreLapéro()))
        {
        }
    }
}