using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class PrendreLapéro : EmptyResponsePartieDeChasseUseCase<Domain.Commands.PrendreLapéro>
    {
        public PrendreLapéro(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository, (partieDeChasse, _) => partieDeChasse.PrendreLapéro(timeProvider))
        {
        }
    }
}