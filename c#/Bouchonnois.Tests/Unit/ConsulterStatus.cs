using Bouchonnois.Domain.Commands;
using Bouchonnois.UseCases;
using Bouchonnois.UseCases.Exceptions;
using FsCheck;
using FsCheck.Xunit;
using static FsCheck.Arb;
using static FsCheck.Prop;

namespace Bouchonnois.Tests.Unit
{
    public class ConsulterStatus : UseCaseTest<UseCases.ConsulterStatus>
    {
        public ConsulterStatus() : base((r, t) => new UseCases.ConsulterStatus(r))
        {
        }

        public class Echoue : UseCaseTest<UseCases.ConsulterStatus>
        {
            private readonly Arbitrary<Guid> _nonExistingPartiesDeChasse = Generate<Guid>().ToArbitrary();

            public Echoue() : base((r, t) => new UseCases.ConsulterStatus(r))

            {
            }

            [Property]
            public Property CarPartieNexistePas()
                => ForAll(
                    _nonExistingPartiesDeChasse,
                    id => MustFailWith<LaPartieDeChasseNexistePas>(
                        () => _useCase.Handle(new Domain.Commands.ConsulterStatus(id)),
                        savedPartieDeChasse => savedPartieDeChasse == null
                    )
                );
        }
    }
}