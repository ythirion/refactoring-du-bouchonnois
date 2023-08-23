using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class TirerSurUneGalinette : PartieDeChasseUseCase<Domain.Tirer.TirerSurUneGalinette, VoidResponse>
    {
        public TirerSurUneGalinette(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository,
                (partieDeChasse, command) =>
                    ToEmpty(partieDeChasse.TirerSurUneGalinette(command.Chasseur)))
        {
        }
    }
}