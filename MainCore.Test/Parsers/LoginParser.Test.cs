namespace MainCore.Test.Parsers
{
    public class LoginParser : BaseParser
    {
        private const string LoginPage = "Parsers/Login/LoginPage.html";
        private const string Buildings = "Parsers/Login/Buildings.html";
        private const string LogoutPage = "Parsers/Login/LogoutPage.html";

        [Theory]
        [InlineData(LoginPage, false)]
        [InlineData(Buildings, true)]
        public void IsIngamePage(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.LoginParser.IsIngamePage(_html);
            actual.ShouldBe(expected);
        }

        [Theory]
        [InlineData(LoginPage, true)]
        [InlineData(LogoutPage, true)]
        [InlineData(Buildings, false)]
        public void IsLoginPage(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.LoginParser.IsLoginPage(_html);
            actual.ShouldBe(expected);
        }

        [Theory]
        [InlineData(LoginPage)]
        [InlineData(LogoutPage)]
        public void GetLoginButton(string file)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.LoginParser.GetLoginButton(_html);
            actual.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(LoginPage)]
        [InlineData(LogoutPage)]
        public void GetUsernameInput(string file)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.LoginParser.GetUsernameInput(_html);
            actual.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(LoginPage)]
        [InlineData(LogoutPage)]
        public void GetPasswordInput(string file)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.LoginParser.GetPasswordInput(_html);
            actual.ShouldNotBeNull();
        }
    }
}