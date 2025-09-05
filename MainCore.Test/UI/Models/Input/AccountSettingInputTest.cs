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
    }
}
