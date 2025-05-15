using Immediate.Handlers.Shared;
using MainCore.Behaviors;
using MainCore.Commands.Features;
using MainCore.Commands.Features.ClaimQuest;
using MainCore.Constraints;
using MainCore.Entities;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace MainCore.Test.Behaviors
{
    public sealed class CommandNameLoggingBehaviorTestHandleBehavior : Behavior<ICommand, ValueTuple>
    {
        public override async ValueTask<ValueTuple> HandleAsync(ICommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return default;
        }
    }

    public class CommandNameLoggingBehaviorTest
    {
        [Theory]
        [MemberData(nameof(Data))]
        public async Task CommandNameLoggingBehaviorShouldLogCorrectName(ICommand command, string expected)
        {
            // Arrange
            var testCorrelatorSinkId = new TestCorrelatorSinkId();

            using var logger = new LoggerConfiguration()
                .WriteTo.TestCorrelator(testCorrelatorSinkId)
                .Enrich.FromLogContext()
                .CreateLogger();

            var commandNameLoggingBehavior = new CommandNameLoggingBehavior<ICommand, ValueTuple>(logger);
            var handleBehavior = new CommandNameLoggingBehaviorTestHandleBehavior();
            commandNameLoggingBehavior.SetInnerHandler(handleBehavior);

            // Act
            using var testCorrelatorContext = TestCorrelator.CreateContext();

            await commandNameLoggingBehavior.HandleAsync(command, CancellationToken.None);

            var logEvent = TestCorrelator.GetLogEventsForSinksFromCurrentContext(testCorrelatorSinkId)[0];

            var nameProperty = logEvent.Properties["name"] as ScalarValue;

            // Assert
            nameProperty!.Value.ShouldBe(expected);
        }

        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { new LoginCommand.Command(new AccountId(3041975)), "Commands.Features.LoginCommand" },
                new object[] { new ClaimQuestCommand.Command(new AccountId(3041975), new VillageId(2091945)), "Commands.Features.ClaimQuest.ClaimQuestCommand" },
            };
    }
}