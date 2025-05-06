using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using MainCore.Constraints;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace MainCore.Test
{
    public class ArchitectureTest
    {
        private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(
            System.Reflection.Assembly.Load("MainCore"),
            typeof(IDbContextFactory<>).Assembly
        ).Build();

        [Fact]
        public void CommandShouldHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(typeof(ICommand))
                .Should()
                .HaveNameEndingWith("Command");

            rule.Check(Architecture);
        }

        [Fact]
        public void QueryShouldHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(typeof(IQuery))
                .Should()
                .HaveNameEndingWith("Query");
            rule.Check(Architecture);
        }

        [Fact]
        public void NotificationShouldHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(typeof(INotification))
                .Should()
                .HaveNameEndingWith("Notification");
            rule.Check(Architecture);
        }

        [Fact]
        public void TaskShouldHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(typeof(ITask))
                .Should()
                .HaveNameEndingWith("Task");

            rule.Check(Architecture);
        }

        [Fact]
        public void HandlerAccessDatabaseShouldNotContainOtherHandler()
        {
            var handler = Classes().That()
                .AreAssignableTo(typeof(Immediate.Handlers.Shared.IHandler<,>))
                .As("Handler");

            var rule = MethodMembers().That()
                .HaveNameContaining("HandleAsync")
                .And()
                .AreStatic()
                .And()
                .DependOnAny(typeof(AppDbContext))
                .Should()
                .NotDependOnAny(handler);
            rule.Check(Architecture);
        }
    }
}