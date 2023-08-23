using Domain.Core;
using FsCheck;
using FsCheck.Xunit;
using static FsCheck.Arb;
using static FsCheck.Prop;

namespace Bouchonnois.Tests.Unit
{
    public class ConsulterStatus : UseCaseTest<UseCases.ConsulterStatus, string>
    {
        public ConsulterStatus() : base((r, _) => new UseCases.ConsulterStatus(r))
        {
        }

        public class Echoue : UseCaseTest<UseCases.ConsulterStatus, string>
        {
            private readonly Arbitrary<Guid> _nonExistingPartiesDeChasse = Generate<Guid>().ToArbitrary();

            public Echoue() : base((r, _) => new UseCases.ConsulterStatus(r))

            {
            }

            [Property]
            public Property CarPartieNexistePas()
                => ForAll(
                    _nonExistingPartiesDeChasse,
                    id => AsyncHelper.RunSync(() => FailWith(
                        $"La partie de chasse {id} n'existe pas",
                        () => UseCase.Handle(new Domain.Consulter.ConsulterStatus(id)),
                        _ => true
                    ))
                );
        }
    }
}