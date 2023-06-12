using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Unit
{
    public abstract class PartieDeChasseServiceTest
    {
        private static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
        protected static readonly Func<DateTime> TimeProvider = () => Now;

        protected readonly PartieDeChasseRepositoryForTests Repository;
        protected readonly PartieDeChasseService PartieDeChasseService;

        protected PartieDeChasseServiceTest()
        {
            Repository = new PartieDeChasseRepositoryForTests();
            PartieDeChasseService = new PartieDeChasseService(Repository, TimeProvider);
        }

        protected static void AssertLastEvent(PartieDeChasse partieDeChasse,
            string expectedMessage)
            => partieDeChasse.Events.Should()
                .HaveCount(1)
                .And
                .EndWith(new Event(Now, expectedMessage));
    }
}