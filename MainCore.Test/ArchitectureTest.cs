using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using MainCore.Constraints;
using MainCore.Entities;
using MainCore.Infrasturecture.Persistence;
using MainCore.Services;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace MainCore.Test
{
    public class ArchitectureTest
    {
        private static readonly Architecture Architecture = new ArchLoader()
            .LoadAssemblies(
            System.Reflection.Assembly.Load("MainCore")
        ).Build();

        private readonly IObjectProvider<MethodMember> Handler =
            MethodMembers().That().HaveNameContaining("HandleAsync").And().AreStatic().As("Handler");

        private readonly IObjectProvider<Class> Request =
            Classes().That().AreAssignableTo(typeof(IConstraint)).As("Request");

        [Fact]
        public void CommandShouldBeRecordAndSealedAndHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(typeof(ICommand))
                .Should()
                .BeRecord()
                .AndShould()
                .BeSealed()
                .AndShould()
                .HaveNameEndingWith("Command");

            rule.Check(Architecture);
        }

        [Fact]
        public void QueryShouldBeRecordAndSealedAndHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(typeof(IQuery))
                .Should()
                .BeRecord()
                .AndShould()
                .BeSealed()
                .AndShould()
                .HaveNameEndingWith("Query");
            rule.Check(Architecture);
        }

        [Fact]
        public void NotificationShouldBeRecordAndSealedAndHaveCorrectName()
        {
            var rule = Classes().That().AreAssignableTo(typeof(INotification))
                .Should()
                .BeRecord()
                .AndShould()
                .BeSealed()
                .AndShould()
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
            var otherhandler = Classes().That()
                .AreAssignableTo(typeof(Immediate.Handlers.Shared.IHandler<,>))
                .As("other handler");

            var rule = MethodMembers().That()
                .Are(Handler)
                .And()
                .DependOnAny(typeof(AppDbContext))
                .Should()
                .NotDependOnAny(otherhandler);
            rule.Check(Architecture);
        }

        [Fact]
        public void HandlerShouldNotDependOnDialogService()
        {
            var rule = MethodMembers().That()
                .Are(Handler)
                .Should()
                .NotDependOnAny(typeof(IDialogService));
            rule.Check(Architecture);
        }

        [Fact]
        public void RequestShouldHaveCorrectConstraint()
        {
            var accountVillageRule = Classes().That()
                .Are(Request)
                .And()
                .DependOnAny(typeof(AccountId))
                .And()
                .DependOnAny(typeof(VillageId))
                .Should()
                .BeAssignableTo(typeof(IAccountVillageConstraint));

            accountVillageRule.Check(Architecture);

            var accountOnlyRule = Classes().That()
                .Are(Request)
                .And()
                .DependOnAny(typeof(AccountId))
                .And()
                .DoNotDependOnAny(typeof(VillageId))
                .Should()
                .BeAssignableTo(typeof(IAccountConstraint));
            accountOnlyRule.Check(Architecture);

            var villageOnlyRule = Classes().That()
                .Are(Request)
                .And()
                .DependOnAny(typeof(VillageId))
                .And()
                .DoNotDependOnAny(typeof(AccountId))
                .Should()
                .BeAssignableTo(typeof(IVillageConstraint));
            villageOnlyRule.Check(Architecture);
        }
    }
}