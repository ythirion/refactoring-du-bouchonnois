using Bouchonnois.Domain;
using LanguageExt;

namespace Bouchonnois.UseCases
{
    public interface IUseCase<in TRequest, TResponse> where TRequest : ICommand
    {
        public EitherAsync<Error, TResponse> Handle(TRequest command);
    }
}