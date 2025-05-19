using Immediate.Handlers.Shared;
using MainCore.Behaviors;
using MainCore.Commands.Features;
using MainCore.Commands.Features.ClaimQuest;
using MainCore.Constraints;
using MainCore.Entities;
using Serilog;
using Serilog.Core;
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

    public class CommandNameLoggingBehaviorTest : IDisposable
    {
        private readonly TestCorrelatorSinkId _testCorrelatorSinkId = new();
        private readonly Logger _logger;

        public CommandNameLoggingBehaviorTest()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.TestCorrelator(_testCorrelatorSinkId)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        public void Dispose()
        {
            _logger.Dispose();
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task CommandNameLoggingBehaviorShouldLogCorrectName(ICommand command, string expected)
        {
            // Arrange

            var behavior = new CommandNameLoggingBehavior<ICommand, ValueTuple>(_logger);
            var handleBehavior = new CommandNameLoggingBehaviorTestHandleBehavior();
            behavior.SetInnerHandler(handleBehavior);

            // Act
            using var testCorrelatorContext = TestCorrelator.CreateContext();

            await behavior.HandleAsync(command, CancellationToken.None);

            var logEvent = TestCorrelator.GetLogEventsForSinksFromCurrentContext(_testCorrelatorSinkId)[0];

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