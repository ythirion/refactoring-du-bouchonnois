using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class Tirer : PartieDeChasseUseCase<Domain.Commands.Tirer, VoidResponse>
    {
        public Tirer(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository,
                (partieDeChasse, command) => ToEmpty(partieDeChasse.Tirer(command.Chasseur, timeProvider)))
        {
        }
    }
}