using ArchUnitNET.xUnitV3;

using Immediate.Handlers.Shared;

using MainCore.Constraints;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace MainCore.Test.ArchitectureTests
{
    public class NamingConventionTests : BaseTest
    {
        [Fact]
        public void Command_ShouldHave_NameEndingWith_Command()
        {
            Classes().That()
                .ImplementInterface(typeof(ICommand))
                .Should().HaveNameEndingWith("Command")
                .Check(Architecture);
        }

        [Fact]
        public void Task_ShouldHave_NameEndingWith_Task()
        {
            Classes().That()
                .ImplementInterface(typeof(ITask))
                .Should().HaveNameEndingWith("Task")
                .Check(Architecture);
        }

        [Fact]
        public void Behavior_ShouldHave_NameEndingWith_Behavior()
        {
            Classes().That()
                .ImplementInterface(typeof(Behavior<,>))
                .Should().HaveNameEndingWith("Behavior")
                .Check(Architecture);
        }
    }
}