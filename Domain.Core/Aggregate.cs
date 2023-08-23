using LanguageExt;

namespace Domain.Core
{
    public abstract class Aggregate : IAggregate, IEquatable<IAggregate>, IEqualityComparer<IAggregate>
    {
        private readonly Func<DateTime> _timeProvider;
        private readonly IRouteEvents _registeredRoutes;
        private readonly ICollection<IEvent> _uncommittedEvents = new LinkedList<IEvent>();

        protected Aggregate(Func<DateTime> timeProvider, bool throwOnApplyNotFound = false)
        {
            _timeProvider = timeProvider;
            _registeredRoutes = new ConventionEventRouter(throwOnApplyNotFound, this);
        }

        public Guid Id { get; protected set; }

        public int Version { get; private set; }

        void IAggregate.ApplyEvent(IEvent @event)
        {
            _registeredRoutes.Dispatch(@event);
            Version++;
        }

        Seq<IEvent> IAggregate.GetUncommittedEvents() => _uncommittedEvents.ToSeq();

        void IAggregate.ClearUncommittedEvents() => _uncommittedEvents.Clear();

        bool IEquatable<IAggregate>.Equals(IAggregate? other) => Equals(other);

        protected void RaiseEvent(IEvent @event)
        {
            ((IAggregate) this).ApplyEvent(@event);
            _uncommittedEvents.Add(@event);
        }

        public override int GetHashCode() => Id.GetHashCode();

        private bool Equals(IAggregate? other) => null != other && other.Id == Id;

        public override bool Equals(object? obj) => Equals(obj as IAggregate);

        protected DateTime Time() => _timeProvider();

        public bool Equals(IAggregate? x, IAggregate? y)
            => x != null && y != null && (ReferenceEquals(x, y) || x.Id.Equals(y.Id));

        public int GetHashCode(IAggregate obj) => GetHashCode();
    }
}