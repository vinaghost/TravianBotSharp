using HtmlAgilityPack;
using MainCore.Commands.Update;
using MainCore.Entities;
using MainCore.Enums;
using MainCore.Services;
using Microsoft.Playwright;

namespace MainCore.Test.Commands.Update
{
    public class UpdateAccountInfoCommandTest : IClassFixture<PlaywrightFixture>, IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;
        private IBrowserContext _context = null!;
        protected IPage Page { get; private set; } = null!;

        public UpdateAccountInfoCommandTest(PlaywrightFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync()
        {
            _context = await _fixture.Browser.NewContextAsync();
            Page = await _context.NewPageAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.CloseAsync();
        }

        private const string PlusAccount = "Parsers/Info/PlusAccount.html";

        [Fact]
        public async Task UpdateAccountInfoCommandShouldRunWithNewAccount()
        {
            // Arrange
            using var context = new FakeDbContextFactory().CreateDbContext(true);
            string htmlContent = await File.ReadAllTextAsync(PlusAccount, TestContext.Current.CancellationToken);
            await Page.SetContentAsync(htmlContent);
            var browser = Substitute.For<IChromeBrowser>();
            browser.CurrentPage.Returns(Page);
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
            string htmlContent = await File.ReadAllTextAsync(PlusAccount, TestContext.Current.CancellationToken);
            var browser = Substitute.For<IChromeBrowser>();
            browser.CurrentPage.Returns(Page);

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