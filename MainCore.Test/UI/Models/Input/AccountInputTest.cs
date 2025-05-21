using MainCore.UI.Models.Input;

namespace MainCore.Test.UI.Models.Input
{
    public class AccountInputTest
    {
        [Fact]
        public void ToDto_UsernameHasNonAlphanumeric()
        {
            // Arrange
            var accountInput = new AccountInput();
            accountInput.Username = "test user @.int";
            // Act
            var dto = accountInput.ToDto();
            // Assert
            dto.Username.ShouldBe("test_user_int");
        }

        [Fact]
        public void ToDto_ServerUrlHasTrail()
        {
            // Arrange
            var accountInput = new AccountInput();
            accountInput.Server = "https://ts1.x1.international.travian.com/dorf1.php";

            // Act
            var dto = accountInput.ToDto();

            // Assert
            dto.Server.ShouldBe("https://ts1.x1.international.travian.com");
        }

        [Fact]
        public void ToDto_ServerUrlDontValid_ReturnEmpty()
        {
            // Arrange
            var accountInput = new AccountInput();
            accountInput.Server = "ts1.x1.international.travian.com/dorf1.php";
            // Act
            var dto = accountInput.ToDto();
            // Assert
            dto.Server.ShouldBe("");
        }
    }
}