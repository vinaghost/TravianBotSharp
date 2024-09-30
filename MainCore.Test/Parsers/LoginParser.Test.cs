namespace MainCore.Test.Parsers
{
    public class LoginParser : BaseParser
    {
        private const string LoginPage = "Parsers/Login/LoginPage.html";
        private const string Buildings = "Parsers/Login/Buildings.html";

        [Theory]
        [InlineData(LoginPage, false)]
        [InlineData(Buildings, true)]
        public void IsIngamePage(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.LoginParser.IsIngamePage(_html);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(LoginPage, true)]
        [InlineData(Buildings, false)]
        public void IsLoginPage(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.LoginParser.IsLoginPage(_html);
            actual.Should().Be(expected);
        }

        [Fact]
        public void GetLoginButton()
        {
            _html.Load(LoginPage);
            var actual = MainCore.Parsers.LoginParser.GetLoginButton(_html);
            actual.Should().NotBeNull();
        }

        [Fact]
        public void GetUsernameInput()
        {
            _html.Load(LoginPage);
            var actual = MainCore.Parsers.LoginParser.GetUsernameInput(_html);
            actual.Should().NotBeNull();
        }

        [Fact]
        public void GetPasswordInput()
        {
            _html.Load(LoginPage);
            var actual = MainCore.Parsers.LoginParser.GetPasswordInput(_html);
            actual.Should().NotBeNull();
        }
    }
}