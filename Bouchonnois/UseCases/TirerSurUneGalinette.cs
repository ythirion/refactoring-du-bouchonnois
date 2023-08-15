using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class TirerSurUneGalinette : EmptyResponsePartieDeChasseUseCase<Domain.Commands.TirerSurUneGalinette>
    {
        public TirerSurUneGalinette(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository,
                (partieDeChasse, command) =>
                    partieDeChasse.TirerSurUneGalinette(command.Chasseur, timeProvider, repository))
        {
        }
    }
}