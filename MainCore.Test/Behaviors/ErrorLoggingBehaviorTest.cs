using FluentResults;
using Immediate.Handlers.Shared;
using MainCore.Behaviors;
using MainCore.Constraints;
using MainCore.Errors;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace MainCore.Test.Behaviors
{
    public sealed class ErrorLoggingBehaviorTestHandleBehavior(Result result) : Behavior<ICommand, IResultBase>
    {
        private readonly Result _result = result;

        public override async ValueTask<IResultBase> HandleAsync(ICommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return _result;
        }
    }

    public class ErrorLoggingBehaviorTest : IDisposable
    {
        private readonly TestCorrelatorSinkId _testCorrelatorSinkId = new();
        private readonly Logger _logger;

        public ErrorLoggingBehaviorTest()
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
        public async Task ErrorLoggingBehaviorShouldLogCorrectErrorMessage(Result result, string expected)
        {
            // Arrange

            var behavior = new ErrorLoggingBehavior<ICommand, IResultBase>(_logger);
            var handleBehavior = new ErrorLoggingBehaviorTestHandleBehavior(result);
            behavior.SetInnerHandler(handleBehavior);

            // Act
            using var testCorrelatorContext = TestCorrelator.CreateContext();

            await behavior.HandleAsync(new Command(), CancellationToken.None);

            var logEvent = TestCorrelator.GetLogEventsForSinksFromCurrentContext(_testCorrelatorSinkId)[0];

            var nameProperty = logEvent.Properties["message"] as ScalarValue;

            // Assert
            nameProperty!.Value.ShouldBe(expected);
        }

        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { Result.Fail(Cancel.Error), "Pause button is pressed" },
                new object[] { Result.Fail(Skip.VillageNotFound), "Village not found" },
                new object[] { Result.Fail(Stop.EnglishRequired("abcxyz")), "Cannot parse abcxyz. Is language English ?. Bot must stop" },
            };
    }
}