using Bouchonnois.Domain;
using Domain.Core;
using FluentAssertions.Primitives;
using static FluentAssertions.Execution.Execute;

namespace Bouchonnois.Tests.Assert
{
    public class PartieDeChasseAssertions : ReferenceTypeAssertions<PartieDeChasse?, PartieDeChasseAssertions>
    {
        protected override string Identifier => "partie de chasse";

        public PartieDeChasseAssertions(PartieDeChasse? partieDeChasse)
            : base(partieDeChasse)
        {
        }

        private AndConstraint<PartieDeChasseAssertions> Call(Action act)
        {
            act();
            return new AndConstraint<PartieDeChasseAssertions>(this);
        }

        public AndConstraint<PartieDeChasseAssertions> HaveEmittedEvent<TEvent>(
            IPartieDeChasseRepository repository,
            TEvent expectedEvent) where TEvent : class, IEvent =>
            Call(() => Assertion
                .Given(() => repository.EventsFor(Subject!.Id))
                .ForCondition(events => AsyncHelper.RunSync(() =>
                    events.Exists(stream => stream.Exists(@event => @event.Equals(expectedEvent)))))
                .FailWith($"Les events devraient contenir {expectedEvent}.")
            );
    }
}