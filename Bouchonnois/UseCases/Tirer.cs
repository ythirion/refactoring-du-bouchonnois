using Bouchonnois.Domain;
using Bouchonnois.Domain.Exceptions;
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

        public Either<Error, VoidResponse> HandleSansException(Domain.Commands.Tirer command,
            Func<DateTime> timeProvider)
        {
            var partieDeChasse = _repository.GetById(command.PartieDeChasseId);

            if (partieDeChasse == null)
                return AnError($"La partie de chasse {command.PartieDeChasseId} n'existe pas");

            try
            {
                partieDeChasse.Tirer(command.Chasseur, timeProvider, _repository);
            }
            catch (ChasseurInconnu)
            {
                return AnError($"Chasseur inconnu {command.Chasseur}");
            }

            return VoidResponse.Empty;
        }
    }
}