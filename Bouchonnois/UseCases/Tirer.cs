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
            PartieDeChasse? foundPartieDeChasse = null;

            var result = _repository
                .GetByIdOption(command.PartieDeChasseId)
                .ToEither(() => AnError($"La partie de chasse {command.PartieDeChasseId} n'existe pas"))
                .Do(p => foundPartieDeChasse = p)
                .Bind(partieDeChasse => partieDeChasse.TirerSansException(command.Chasseur, _timeProvider))
                .Map(_ => VoidResponse.Empty);

            if (foundPartieDeChasse != null) _repository.Save(foundPartieDeChasse);

            return result;
        }
    }
}