using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace MainCore.Test
{
    public class ArchitectureTest
    {
        private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(
            System.Reflection.Assembly.Load("MainCore")
        ).Build();

        private readonly IObjectProvider<Interface> CommandInterfaces =
            Interfaces().That().HaveNameEndingWith("Command").As("Command Interfaces");

        private readonly IObjectProvider<Interface> QueryInterfaces =
            Interfaces().That().HaveNameEndingWith("Query").As("Query Interfaces");

        private readonly IObjectProvider<Interface> NotificationInterfaces =
            Interfaces().That().HaveNameEndingWith("Notification").As("Notification Interfaces");

        private readonly IObjectProvider<Interface> TaskInterfaces =
            Interfaces().That().HaveNameEndingWith("Task").As("Task Interfaces");

        [Fact]
        public void CommandShouldHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(CommandInterfaces)
                .Should()
                .HaveNameEndingWith("Command");

            rule.Check(Architecture);
        }

        [Fact]
        public void QueryShouldHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(QueryInterfaces)
                .Should()
                .HaveNameEndingWith("Query");
            rule.Check(Architecture);
        }

        [Fact]
        public void NotificationShouldHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(NotificationInterfaces)
                .Should()
                .HaveNameEndingWith("Notification");
            rule.Check(Architecture);
        }

        [Fact]
        public void TaskShouldHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(TaskInterfaces)
                .Should()
                .HaveNameEndingWith("Task");
            rule.Check(Architecture);
        }
    }
}