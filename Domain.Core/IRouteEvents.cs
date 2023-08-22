namespace Domain.Core
{
    public interface IRouteEvents
    {
        void Register(IAggregate aggregate);
        void Dispatch(object eventMessage);
    }
}