using ArchUnitNET.Fluent.Syntax.Elements.Types;
using Bouchonnois.Domain;
using static Bouchonnois.Tests.Architecture.ArchUnitExtensions;

namespace Bouchonnois.Tests.Architecture
{
    public class ArchitectureRules
    {
        private static GivenTypesConjunctionWithDescription ApplicationServices() =>
            TypesInAssembly().And()
                .ResideInNamespace("Service", true)
                .As("Application Services");

        private static GivenTypesConjunctionWithDescription DomainModel() =>
            TypesInAssembly().And()
                .ResideInNamespace("Domain", true)
                .As("Domain Model");

        private static GivenTypesConjunctionWithDescription Infrastructure() =>
            TypesInAssembly().And()
                .ResideInNamespace("Repository", true)
                .As("Infrastructure");

        [Fact]
        public void ApplicationServicesRules() =>
            ApplicationServices().Should()
                .NotDependOnAny(Infrastructure())
                .Check();

        [Fact]
        public void InfrastructureRules() =>
            Infrastructure().Should()
                .ImplementInterface(typeof(IPartieDeChasseRepository))
                .Check();

        [Fact]
        public void DomainModelRules() =>
            DomainModel().Should()
                .NotDependOnAny(ApplicationServices()).AndShould()
                .NotDependOnAny(Infrastructure())
                .Check();
    }
}