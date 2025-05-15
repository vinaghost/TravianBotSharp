using Immediate.Handlers.Shared;
using MainCore.Behaviors;
using MainCore.Constraints;
using MainCore.Entities;
using MainCore.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace MainCore.Test.Behaviors
{
    public sealed class AccountDataLoggingBehaviorTestHandleBehavior(ILogger logger) : Behavior<IAccountConstraint, ValueTuple>
    {
        private readonly ILogger _logger = logger;

        public override async ValueTask<ValueTuple> HandleAsync(IAccountConstraint request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            _logger.Information("Test log message");
            return default;
        }
    }

    public class AccountDataLoggingBehaviorTest
    {
        [Fact]
        public async Task AccountDataLoggingBehaviorShouldAddCorrectPropertyInLogContext()
        {
            // Arrange
            var testCorrelatorSinkId = new TestCorrelatorSinkId();
            var accountId = new AccountId(3041975);
            var accountData = "TestAccountData";

            using var logger = new LoggerConfiguration()
                .WriteTo.TestCorrelator(testCorrelatorSinkId)
                .Enrich.FromLogContext()
                .CreateLogger();

            var dataService = Substitute.For<IDataService>();
            dataService.AccountId.Returns(accountId);
            dataService.AccountData.Returns(accountData);

            var accountDataLoggingBehavior = new AccountDataLoggingBehavior<IAccountConstraint, ValueTuple>(logger, dataService);
            var handleBehavior = new AccountDataLoggingBehaviorTestHandleBehavior(logger);
            accountDataLoggingBehavior.SetInnerHandler(handleBehavior);

            // Act
            using var testCorrelatorContext = TestCorrelator.CreateContext();

            await accountDataLoggingBehavior.HandleAsync(new AccountConstraint(accountId), CancellationToken.None);

            var logEvent = TestCorrelator.GetLogEventsForSinksFromCurrentContext(testCorrelatorSinkId)[0];

            var accountIdProperty = logEvent.Properties["AccountId"] as ScalarValue;
            var accountDataProperty = logEvent.Properties["Account"] as ScalarValue;

            // Assert
            accountIdProperty!.Value.ShouldBe(accountId.ToString());
            accountDataProperty!.Value.ShouldBe(accountData.ToString());
        }
    }
}