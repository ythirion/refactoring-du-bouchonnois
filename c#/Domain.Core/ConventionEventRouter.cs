using System.Reflection;
using LanguageExt;

namespace Domain.Core
{
    internal sealed class ConventionEventRouter : IRouteEvents
    {
        private readonly IDictionary<Type, Action<object>?> _handlers = new Dictionary<Type, Action<object>?>();
        private readonly bool _throwOnApplyNotFound;
        private IAggregate? _registered;

        public ConventionEventRouter(bool throwOnApplyNotFound, IAggregate aggregate)
        {
            _throwOnApplyNotFound = throwOnApplyNotFound;
            Register(aggregate);
        }

        public void Register(IAggregate aggregate)
            => aggregate.Let(agg =>
            {
                _registered = agg;
                ApplyMethodsFor(agg)
                    .Iter((_, method) => RegisterMethodOnAggregate(agg, method));
            });

        private static Arr<(MethodInfo infos, Type eventType)> ApplyMethodsFor(IAggregate aggregate)
            => aggregate
                .GetType()
                .GetMethods(BindingFlags.Default
                            | BindingFlags.Instance
                            | BindingFlags.NonPublic
                            | BindingFlags.Public)
                .Where(m => m.GetCustomAttribute<EventSourcedAttribute>() != null
                            && m.GetParameters().Length == 1
                            && m.ReturnParameter.ParameterType == typeof(void))
                .Map(m => (m, m.GetParameters().Single().ParameterType))
                .ToArr();

        private void RegisterMethodOnAggregate(IAggregate aggregate, (MethodInfo infos, Type eventType) method)
            => _handlers.Add(method.eventType, m => method.infos.Invoke(aggregate, new[] {m}));

        public void Dispatch(object eventMessage)
            => _handlers.Find(e => e.Key == eventMessage.GetType())
                .Do(result => result.Value?.Invoke(eventMessage))
                .IfNone(() =>
                {
                    if (_throwOnApplyNotFound)
                    {
                        throw new ArgumentException(
                            $"Aggregate of type '{_registered?.GetType().Name!}' raised an event of type '{eventMessage.GetType().Name}' but no handler could be found to handle the event."
                        );
                    }
                });
    }
}