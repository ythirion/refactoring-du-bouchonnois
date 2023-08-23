using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent.Syntax.Elements.Members.MethodMembers;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Classes;
using Domain.Core;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using static LanguageExt.List;
using ICommand = Bouchonnois.Domain.Commands.ICommand;

namespace Bouchonnois.Tests.Architecture
{
    public class Guidelines
    {
        private static GivenMethodMembersThat Methods() => MethodMembers().That().AreNoConstructors().And();

        [Fact]
        public void NoGetMethodShouldReturnVoid() =>
            Methods()
                .HaveName("Get[A-Z].*", useRegularExpressions: true).Should()
                .NotHaveReturnType(typeof(void))
                .Check();

        [Fact]
        public void IserAndHaserShouldReturnBooleans() =>
            Methods()
                .HaveName("Is[A-Z].*", useRegularExpressions: true).Or()
                .HaveName("Has[A-Z].*", useRegularExpressions: true).Should()
                .HaveReturnType(typeof(bool))
                .Check();

        [Fact]
        public void SettersShouldNotReturnSomething() =>
            Methods()
                .HaveName("Set[A-Z].*", useRegularExpressions: true).Should()
                .HaveReturnType(typeof(void))
                .Check();

        [Fact]
        public void InterfacesShouldStartWithI() =>
            Interfaces().Should()
                .HaveName("^I[A-Z].*", useRegularExpressions: true)
                .Because("C# convention...")
                .Check();

        private readonly GivenClassesConjunction _commands = Classes().That()
            .ImplementInterface(typeof(ICommand)).Or()
            .HaveNameEndingWith("Command");

        private readonly GivenClassesConjunction _events = Classes().That().ImplementInterface(typeof(IEvent));

        [Fact]
        public void CommandsAndEventsShouldBePartOfDomain()
            => create(_commands, _events)
                .ForEach(ShouldBePartOfDomain);

        private static void ShouldBePartOfDomain(GivenClassesConjunction classes)
            => classes.Should()
                .ResideInNamespace("Domain", true)
                .Check();
    }
}