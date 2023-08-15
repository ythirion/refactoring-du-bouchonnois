using Bouchonnois.Domain;
using static Bouchonnois.UseCases.VoidResponse;

namespace Bouchonnois.UseCases
{
    public sealed class Tirer : PartieDeChasseUseCase<Domain.Commands.Tirer, VoidResponse>
    {
        public Tirer(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository, (partieDeChasse, command) =>
            {
                partieDeChasse.Tirer(command.Chasseur, timeProvider, repository);
                return Empty;
            })
        {
        }
    }
}