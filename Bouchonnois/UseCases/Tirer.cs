using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class Tirer : PartieDeChasseUseCase<Domain.Tirer.Tirer, VoidResponse>
    {
        public Tirer(IPartieDeChasseRepository repository)
            : base(repository,
                (partieDeChasse, command) => ToEmpty(partieDeChasse.Tirer(command.Chasseur)))
        {
        }
    }
}