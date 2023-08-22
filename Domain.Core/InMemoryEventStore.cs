using System.Reflection;
using LanguageExt;

namespace Domain.Core
{
    public sealed class InMemoryEventStore : IEventStore
    {
        private Map<string, Seq<IEvent>> _eventStream;
        private readonly Func<DateTime> _timeProvider;

        public InMemoryEventStore(Func<DateTime> timeProvider)
        {
            _eventStream = new Map<string, Seq<IEvent>>();
            _timeProvider = timeProvider;
        }

        private TAggregate CreateAggregate<TAggregate>(Guid id) where TAggregate : class, IAggregate
            => (TAggregate) typeof(TAggregate)
                .GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] {typeof(Guid), typeof(Func<DateTime>)},
                    null
                )!.Invoke(new object[] {id, _timeProvider});

        private static string KeyFor(Type type, Guid id) => $"{type.FullName}_{id}";
        private static string KeyFor<TAggregate>(Guid id) => KeyFor(typeof(TAggregate), id);
        private static string KeyFor(IAggregate aggregate) => KeyFor(aggregate.GetType(), aggregate.Id);

        public OptionAsync<Seq<IEvent>> GetEventsById<TAggregate>(Guid id) where TAggregate : class, IAggregate
            => _eventStream.Find(KeyFor<TAggregate>(id)).ToAsync();

        public OptionAsync<TAggregate> GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
            => GetEventsById<TAggregate>(id)
                .Map(events =>
                    CreateAggregate<TAggregate>(id).Let(agg => ApplyEvents(agg, events))
                );

        private static void ApplyEvents(IAggregate aggregate, IEnumerable<IEvent> events)
            => events.ToList().ForEach(aggregate.ApplyEvent);

        public Task Save<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate
            => Task.Run(() =>
                {
                    _eventStream = _eventStream.AddOrUpdate(
                        KeyFor(aggregate),
                        some => some.Append(aggregate.GetUncommittedEvents()).ToSeq(),
                        () => aggregate.GetUncommittedEvents().ToSeq()
                    );

                    aggregate.ClearUncommittedEvents();
                }
            );
    }
}