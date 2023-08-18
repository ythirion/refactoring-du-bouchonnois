using Bouchonnois.Domain;
using Bouchonnois.Domain.Commands;
using LanguageExt;

namespace Bouchonnois.UseCases
{
    public interface IUseCase<in TRequest, TResponse> where TRequest : ICommand
    {
        public Either<Error, TResponse> Handle(TRequest command);
    }
}