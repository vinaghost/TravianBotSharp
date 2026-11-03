using ArchUnitNET.xUnitV3;

using Immediate.Handlers.Shared;

using MainCore.Infrasturecture.Persistence;
using MainCore.Services;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace MainCore.Test.ArchitectureTests
{
    public class DependencyGuardTests : BaseTest
    {
        [Fact]
        public void Handler_ShouldNotDependOn_AppDbContext()
        {
            Classes().That()
                .ImplementInterface(typeof(IHandler<,>)).Should()
                .NotDependOnAny(typeof(AppDbContext))
                .Check(Architecture);
        }

        [Fact]
        public void Handler_ShouldNotDependOn_ChromeBrowser()
        {
            Classes().That()
                .ImplementInterface(typeof(IHandler<,>)).Should()
                .NotDependOnAny(typeof(IChromeBrowser))
                .Check(Architecture);
        }
    }
}