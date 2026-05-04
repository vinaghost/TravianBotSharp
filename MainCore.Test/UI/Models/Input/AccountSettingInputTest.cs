using MainCore.Enums;
using MainCore.UI.Models.Input;
using System.Reflection;

namespace MainCore.Test.UI.Models.Input
{
    public class AccountSettingInputTest
    {
        [Fact]
        public void Get_ReturnsDictionaryWithCorrectCount()
        {
            // Arrange
            var accountSettingInput = new AccountSettingInput();

            // Act
            var result = accountSettingInput.Get();

            // Assert that every returned key corresponds to a valid enum value and
            // that deprecated settings (SleepTimeMin/Max) are not exposed in the UI.
            foreach (var key in result.Keys)
            {
                Assert.True(Enum.IsDefined(typeof(AccountSettingEnums), key));
            }
            Assert.DoesNotContain(AccountSettingEnums.SleepTimeMin, result.Keys);
            Assert.DoesNotContain(AccountSettingEnums.SleepTimeMax, result.Keys);
        }

        [Fact]
        public void MinutesSettings_ArePersistedThroughDictionary()
        {
            var input = new AccountSettingInput();
            input.WorkStartHour.Set(3);
            input.WorkStartMinute.Set(15);
            input.WorkEndHour.Set(20);
            input.WorkEndMinute.Set(45);
            input.RandomMinute.Set(12);

            var dict = input.Get();
            Assert.Equal(3, dict[AccountSettingEnums.WorkStartHour]);
            Assert.Equal(15, dict[AccountSettingEnums.WorkStartMinute]);
            Assert.Equal(20, dict[AccountSettingEnums.WorkEndHour]);
            Assert.Equal(45, dict[AccountSettingEnums.WorkEndMinute]);
            Assert.Equal(12, dict[AccountSettingEnums.SleepRandomMinute]);

            // feeding back through Set should keep values
            var input2 = new AccountSettingInput();
            input2.Set(dict);
            Assert.Equal(3, input2.WorkStartHour.Get());
            Assert.Equal(15, input2.WorkStartMinute.Get());
            Assert.Equal(20, input2.WorkEndHour.Get());
            Assert.Equal(45, input2.WorkEndMinute.Get());
            Assert.Equal(12, input2.RandomMinute.Get());
        }

        [Fact]
        public async Task SaveAccountSettingCommand_UpsertsNewValues()
        {
            // arrange in-memory context with a single account
            var factory = new FakeDbContextFactory();
            await using var context = factory.CreateDbContext(true);
            var accountId = context.Accounts.Select(a => a.Id).First();

            var settings = new Dictionary<AccountSettingEnums, int>
            {
                { AccountSettingEnums.WorkStartHour, 8 },
                { AccountSettingEnums.WorkStartMinute, 30 }
            };
            var command = new MainCore.Commands.UI.Misc.SaveAccountSettingCommand.Command(new(accountId), settings);

            // invoke private static handler via reflection
            var method = typeof(MainCore.Commands.UI.Misc.SaveAccountSettingCommand)
                .GetMethod("HandleAsync", BindingFlags.NonPublic | BindingFlags.Static)!;
            var task = (ValueTask)method.Invoke(null, new object[] { command, context, null! })!;
            await task;

            // assert rows exist with correct values
            var row1 = context.AccountsSetting.FirstOrDefault(x => x.AccountId == accountId && x.Setting == AccountSettingEnums.WorkStartHour);
            var row2 = context.AccountsSetting.FirstOrDefault(x => x.AccountId == accountId && x.Setting == AccountSettingEnums.WorkStartMinute);
            Assert.NotNull(row1);
            Assert.NotNull(row2);
            Assert.Equal(8, row1.Value);
            Assert.Equal(30, row2.Value);
        }
    }
}