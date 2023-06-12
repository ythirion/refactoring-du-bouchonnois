using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;
using FluentAssertions.Specialized;

namespace Bouchonnois.Tests.Unit
{
    public abstract class PartieDeChasseServiceTest
    {
        protected static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
        protected static readonly Func<DateTime> TimeProvider = () => Now;

        protected readonly PartieDeChasseRepositoryForTests Repository;
        protected readonly PartieDeChasseService PartieDeChasseService;

        protected PartieDeChasseServiceTest()
        {
            Repository = new PartieDeChasseRepositoryForTests();
            PartieDeChasseService = new PartieDeChasseService(Repository, TimeProvider);
        }

        protected PartieDeChasse UnePartieDeChasseExistante(PartieDeChasseBuilder partieDeChasseBuilder)
        {
            var partieDeChasse = partieDeChasseBuilder.Build();
            Repository.Add(partieDeChasse);

            return partieDeChasse;
        }

        protected static void AssertLastEvent(PartieDeChasse partieDeChasse,
            string expectedMessage)
            => partieDeChasse.Events.Should()
                .HaveCount(1)
                .And
                .EndWith(new Event(Now, expectedMessage));

        protected PartieDeChasse? SavedPartieDeChasse() => Repository.SavedPartieDeChasse();

        protected ExceptionAssertions<TException> ExecuteAndAssertThrow<TException>(Action<PartieDeChasseService> act,
            Action<PartieDeChasse?> assert)
            where TException : Exception
        {
            var ex = ((Action) (() => act(PartieDeChasseService))).Should().Throw<TException>();
            assert(SavedPartieDeChasse());

            return ex;
        }
    }
}