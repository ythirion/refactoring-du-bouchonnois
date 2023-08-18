using Bouchonnois.Domain;
using LanguageExt;
using static Bouchonnois.Domain.Error;

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
            AnError($"La partie de chasse {command.PartieDeChasseId} n'existe pas");
    }
}