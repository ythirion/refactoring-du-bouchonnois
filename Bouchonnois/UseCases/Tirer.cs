using Bouchonnois.Domain;
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

            var result = partieDeChasse
                .TirerSansException(command.Chasseur, _timeProvider)
                .Map(_ => VoidResponse.Empty);

            _repository.Save(partieDeChasse);

            return result;
        }
    }
}