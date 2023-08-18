using Bouchonnois.Domain;
using Bouchonnois.Domain.Exceptions;
using LanguageExt;
using static Bouchonnois.Domain.Error;

namespace Bouchonnois.UseCases
{
    public sealed class Tirer : EmptyResponsePartieDeChasseUseCase<Domain.Commands.Tirer>
    {
        private readonly Func<DateTime> _timeProvider;

        public Tirer(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
            : base(repository,
                (partieDeChasse, command) => partieDeChasse.Tirer(command.Chasseur, timeProvider, repository))
        {
            _timeProvider = timeProvider;
        }

        public Either<Error, VoidResponse> HandleSansException(Domain.Commands.Tirer command)
        {
            var partieDeChasse = _repository.GetById(command.PartieDeChasseId);

            if (partieDeChasse == null)
                return AnError($"La partie de chasse {command.PartieDeChasseId} n'existe pas");

            try
            {
                partieDeChasse.Tirer(command.Chasseur, _timeProvider, _repository);
            }
            catch (ChasseurInconnu)
            {
                return AnError($"Chasseur inconnu {command.Chasseur}");
            }
            catch (TasPlusDeBallesMonVieuxChasseALaMain)
            {
                return AnError($"{command.Chasseur} tire -> T'as plus de balles mon vieux, chasse à la main");
            }

            return VoidResponse.Empty;
        }
    }
}