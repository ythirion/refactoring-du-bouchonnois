namespace Domain.Core
{
    internal interface IRouteEvents
    {
        void Register(IAggregate aggregate);
        void Dispatch(object eventMessage);
    }
}