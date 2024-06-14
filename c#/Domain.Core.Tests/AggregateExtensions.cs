using LanguageExt;

namespace Domain.Core.Tests
{
    public static class AggregateExtensions
    {
        public static bool HasRaisedEvent<TEvent>(this IAggregate aggregate, TEvent @event)
            where TEvent : class, IEvent
            => aggregate.GetUncommittedEvents().Exists(e => e.Equals(@event));

        public static Seq<IEvent> UncommittedEvents(this IAggregate aggregate)
            => aggregate.GetUncommittedEvents();
    }
}