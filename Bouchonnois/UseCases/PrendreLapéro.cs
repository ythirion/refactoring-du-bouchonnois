using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class PrendreLapéro : PartieDeChasseUseCase<Domain.Apéro.PrendreLapéro, VoidResponse>
    {
        public PrendreLapéro(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository,
                (partieDeChasse, _) => ToEmpty(partieDeChasse.PrendreLapéro(timeProvider)))
        {
        }
    }
}