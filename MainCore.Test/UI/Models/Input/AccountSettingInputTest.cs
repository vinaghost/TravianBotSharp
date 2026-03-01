using MainCore.Enums;
using MainCore.UI.Models.Input;

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

            // Assert
            var enumCount = Enum.GetValues(typeof(AccountSettingEnums)).Length;
            Assert.Equal(enumCount, result.Count);
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
    }
}