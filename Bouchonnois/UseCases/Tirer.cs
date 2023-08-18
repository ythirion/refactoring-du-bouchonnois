using Bouchonnois.Domain;
using LanguageExt;

namespace Bouchonnois.UseCases
{
    public sealed class Tirer : EmptyResponsePartieDeChasseUseCase<Domain.Commands.Tirer>
    {
        public Tirer(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository,
                (partieDeChasse, command) => partieDeChasse.Tirer(command.Chasseur, timeProvider, repository))
        {
        }

        public Either<Error, VoidResponse> HandleSansException(Domain.Commands.Tirer command) =>
            new Error($"La partie de chasse {command.PartieDeChasseId} n'existe pas");
    }
}