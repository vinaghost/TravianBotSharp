using MainCore.Commands.Queries;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Services;

namespace MainCore.Test.Commands.Misc
{
    public class DelayCommand
    {
        private readonly IDataService _dataService = Substitute.For<IDataService>();
        private readonly IGetSetting _getSetting = Substitute.For<IGetSetting>();

        public DelayCommand()
        {
            _getSetting.ByName(Arg.Any<AccountId>(), Arg.Any<AccountSettingEnums>(), Arg.Any<AccountSettingEnums>()).Returns(1000);
        }

        [Fact]
        public async Task DelayClickCommand_ShouldReturnCancelError_WhenTokenWasCanceled()
        {
            MainCore.Commands.Misc.DelayClickCommand delayClickCommand = new(_dataService, _getSetting);
            // Arrange
            var cts = new CancellationTokenSource();
            cts.CancelAfter(200);
            // Act
            var result = await delayClickCommand.Execute(cts.Token);
            cts.Dispose();

            // Assert
            result.IsFailed.Should().Be(true);
            result.HasError<Cancel>().Should().Be(true);
        }

        [Fact]
        public async Task DelayTaskCommand_ShouldReturnCancelError_WhenTokenWasCanceled()
        {
            MainCore.Commands.Misc.DelayTaskCommand delayTaskCommand = new(_dataService, _getSetting);
            // Arrange
            var cts = new CancellationTokenSource();
            cts.CancelAfter(200);
            // Act
            var result = await delayTaskCommand.Execute(cts.Token);
            cts.Dispose();

            // Assert
            result.IsFailed.Should().Be(true);
            result.HasError<Cancel>().Should().Be(true);
        }
    }
}