using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class Tirer : EmptyResponsePartieDeChasseUseCase<Domain.Commands.Tirer>
    {
        public Tirer(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository,
                (partieDeChasse, command) => partieDeChasse.Tirer(command.Chasseur, timeProvider, repository))
        {
        }
    }
}