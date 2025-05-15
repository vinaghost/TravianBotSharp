using FluentResults;
using Immediate.Handlers.Shared;
using MainCore.Behaviors;
using MainCore.Constraints;
using MainCore.Errors;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace MainCore.Test.Behaviors
{
    public sealed class ErrorLoggingBehaviorTestHandleBehavior(Result result) : Behavior<IConstraint, Result>
    {
        private readonly Result _result = result;

        public override async ValueTask<Result> HandleAsync(IConstraint request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return _result;
        }
    }

    public class ErrorLoggingBehaviorTest
    {
        [Theory]
        [MemberData(nameof(Data))]
        public async Task CommandNameLoggingBehaviorShouldLogCorrectName(Result result, string expected)
        {
            // Arrange
            var testCorrelatorSinkId = new TestCorrelatorSinkId();

            using var logger = new LoggerConfiguration()
                .WriteTo.TestCorrelator(testCorrelatorSinkId)
                .Enrich.FromLogContext()
                .CreateLogger();

            var commandNameLoggingBehavior = new ErrorLoggingBehavior<IConstraint, Result>(logger);
            var handleBehavior = new ErrorLoggingBehaviorTestHandleBehavior(result);
            commandNameLoggingBehavior.SetInnerHandler(handleBehavior);

            // Act
            using var testCorrelatorContext = TestCorrelator.CreateContext();

            await commandNameLoggingBehavior.HandleAsync(new Constraint(), CancellationToken.None);

            var logEvent = TestCorrelator.GetLogEventsForSinksFromCurrentContext(testCorrelatorSinkId)[0];

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