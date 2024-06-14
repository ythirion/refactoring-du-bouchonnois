using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class TerminerLaPartie : PartieDeChasseUseCase<Domain.Terminer.TerminerLaPartie, string>
    {
        public TerminerLaPartie(IPartieDeChasseRepository repository)
            : base(repository, (partieDeChasse, _) => partieDeChasse.Terminer())
        {
        }
    }
}