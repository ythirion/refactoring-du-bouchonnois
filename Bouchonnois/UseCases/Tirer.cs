using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class Tirer : PartieDeChasseUseCase<Domain.Commands.Tirer, VoidResponse>
    {
        public Tirer(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository,
                (partieDeChasse, command) => partieDeChasse
                    .Tirer(command.Chasseur, timeProvider)
                    .Map(_ => VoidResponse.Empty))
        {
        }
    }
}