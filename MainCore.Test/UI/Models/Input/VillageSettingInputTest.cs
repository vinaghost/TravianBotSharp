using MainCore.Enums;
using MainCore.UI.Models.Input;

namespace MainCore.Test.UI.Models.Input
{
    public class VillageSettingInputTest
    {
        [Fact]
        public void Get_ReturnsDictionaryWithCorrectCount()
        {
            // Arrange
            var villageSettingInput = new VillageSettingInput();

            // Act
            var result = villageSettingInput.Get();

            // Assert
            var enumCount = Enum.GetValues(typeof(VillageSettingEnums)).Length;
            Assert.Equal(enumCount, result.Count);
        }
    }
}
