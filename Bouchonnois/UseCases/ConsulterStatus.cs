using Bouchonnois.Domain;

namespace Bouchonnois.UseCases
{
    public sealed class ConsulterStatus : PartieDeChasseUseCase<Domain.Commands.ConsulterStatus, string>
    {
        public ConsulterStatus(IPartieDeChasseRepository repository) :
            base(repository, (partieDeChasse, _) => partieDeChasse.Consulter())
        {
        }
    }
}