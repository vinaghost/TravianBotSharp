using HtmlAgilityPack;
using MainCore.Commands.Update;
using MainCore.Entities;
using MainCore.Enums;
using MainCore.Services;

namespace MainCore.Test.Commands.Update
{
    public class UpdateAccountInfoCommandTest
    {
        private const string PlusAccount = "Parsers/Info/PlusAccount.html";

        [Fact]
        public async Task UpdateAccountInfoCommandShouldRunWithNewAccount()
        {
            // Arrange
            using var context = new FakeDbContextFactory().CreateDbContext(true);
            var html = new HtmlDocument();
            html.Load(PlusAccount);
            var browser = Substitute.For<IChromeBrowser>();
            browser.Html.Returns(html);
            var handleBehavior = new UpdateAccountInfoCommand.HandleBehavior(browser, context);

            var command = new UpdateAccountInfoCommand.Command(new AccountId(1));

            // Act
            await handleBehavior.HandleAsync(command, CancellationToken.None);

            // Assert
            var accountInfo = context.AccountsInfo.FirstOrDefault(x => x.AccountId == 1);
            accountInfo.ShouldNotBeNull();
            accountInfo.Gold.ShouldBeGreaterThanOrEqualTo(0);
            accountInfo.Silver.ShouldBeGreaterThanOrEqualTo(0);
            accountInfo.HasPlusAccount.ShouldBeTrue();
        }

        [Fact]
        public async Task UpdateAccountInfoCommandShouldRunWithExistingAccount()
        {
            // Arrange
            using var context = new FakeDbContextFactory().CreateDbContext(true);
            context.Add(new AccountInfo
            {
                AccountId = 1,
                Gold = 0,
                Silver = 0,
                HasPlusAccount = false,
                Tribe = TribeEnums.Any,
            });
            context.SaveChanges();
            var html = new HtmlDocument();
            html.Load(PlusAccount);
            var browser = Substitute.For<IChromeBrowser>();
            browser.Html.Returns(html);
            var handleBehavior = new UpdateAccountInfoCommand.HandleBehavior(browser, context);
            var command = new UpdateAccountInfoCommand.Command(new AccountId(1));

            // Act
            await handleBehavior.HandleAsync(command, CancellationToken.None);

            // Assert
            var accountInfo = context.AccountsInfo.FirstOrDefault(x => x.AccountId == 1);
            accountInfo.ShouldNotBeNull();
            accountInfo.Gold.ShouldBeGreaterThanOrEqualTo(0);
            accountInfo.Silver.ShouldBeGreaterThanOrEqualTo(0);
            accountInfo.HasPlusAccount.ShouldBeTrue();
        }
    }
}