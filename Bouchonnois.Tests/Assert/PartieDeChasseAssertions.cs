using Bouchonnois.Domain;
using Bouchonnois.Tests.Doubles;
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

        public void HaveEmittedEvent<TEvent>(PartieDeChasseRepositoryForTests repository,
            TEvent expectedEvent) where TEvent : class, IEvent
            => Call(() =>
                Assertion.Given(() => Subject!.Id)
                    .ForCondition(_ => repository.LastEvent().Equals(expectedEvent))
                    .FailWith($"Les events devraient contenir {expectedEvent}.")
            );
    }
}