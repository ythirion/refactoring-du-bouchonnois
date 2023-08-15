using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class TirerSurUneGalinette : PartieDeChasseUseCase<Domain.Commands.TirerSurUneGalinette, VoidResponse>
    {
        public TirerSurUneGalinette(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository, (partieDeChasse, command) =>
            {
                partieDeChasse.TirerSurUneGalinette(command.Chasseur, timeProvider, repository);
                return VoidResponse.Empty;
            })
        {
        }
    }
}