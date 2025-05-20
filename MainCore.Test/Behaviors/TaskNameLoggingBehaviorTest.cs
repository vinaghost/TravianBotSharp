using Immediate.Handlers.Shared;
using MainCore.Behaviors;
using MainCore.Constraints;
using MainCore.Entities;
using MainCore.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace MainCore.Test.Behaviors
{
    public sealed class TaskNameLoggingBehaviorTestHandleBehavior : Behavior<ITask, ValueTuple>
    {
        public override async ValueTask<ValueTuple> HandleAsync(ITask request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return default;
        }
    }

    public class TaskNameLoggingBehaviorTest : IDisposable
    {
        private readonly TestCorrelatorSinkId _testCorrelatorSinkId = new();
        private readonly Logger _logger;

        public TaskNameLoggingBehaviorTest()
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
        public async Task CommandNameLoggingBehaviorShouldLogCorrectName(ITask task, string expected)
        {
            // Arrange

            var behavior = new TaskNameLoggingBehavior<ITask, ValueTuple>(_logger);
            var handleBehavior = new TaskNameLoggingBehaviorTestHandleBehavior();
            behavior.SetInnerHandler(handleBehavior);

            // Act
            using var testCorrelatorContext = TestCorrelator.CreateContext();

            await behavior.HandleAsync(task, CancellationToken.None);

            var logEvent = TestCorrelator.GetLogEventsForSinksFromCurrentContext(_testCorrelatorSinkId)[0];

            var nameProperty = logEvent.Properties["TaskName"] as ScalarValue;

            // Assert
            nameProperty!.Value.ShouldBe(expected);
        }

        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { new LoginTask.Task(new AccountId(3041975)), "Login" },
                new object[] { new ClaimQuestTask.Task(new AccountId(3041975), new VillageId(2091945), "Test village"), "Claim quest in Test village" },
            };
    }
}