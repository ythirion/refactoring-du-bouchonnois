using Bouchonnois.Domain.Commands;

namespace Bouchonnois.UseCases
{
    public interface IUseCase<in TRequest, out TResponse> where TRequest : ICommand
    {
        public TResponse Handle(TRequest command);
    }
}