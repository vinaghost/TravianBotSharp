using Microsoft.Playwright;

namespace MainCore.Test.Parsers
{
    public abstract class ParserTestBase(PlaywrightFixture fixture) : IClassFixture<PlaywrightFixture>
    {
        protected PlaywrightFixture Fixture { get; } = fixture;

        protected Task<IBrowserContext> CreateContextAsync()
        {
            return Fixture.Browser.NewContextAsync();
        }

        protected static async Task<IPage> CreatePageAsync(IBrowserContext context, string relativeHtmlPath)
        {
            var page = await context.NewPageAsync();

            var fullPath = Path.Combine(AppContext.BaseDirectory, "HtmlFiles", relativeHtmlPath);
            var html = await File.ReadAllTextAsync(fullPath);
            await page.SetContentAsync(html);

            return page;
        }
    }
}