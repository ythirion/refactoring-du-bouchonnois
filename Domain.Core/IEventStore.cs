using LanguageExt;

namespace Domain.Core
{
    public interface IEventStore
    {
        OptionAsync<Seq<IEvent>> GetEventsById<TAggregate>(Guid id) where TAggregate : class, IAggregate;
        OptionAsync<TAggregate> GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate;
        Task Save<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate;
    }
}