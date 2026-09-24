using Microsoft.Playwright;

namespace MainCore.Test.Parsers
{
    public abstract class BaseParser : IClassFixture<PlaywrightFixture>, IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;
        private IBrowserContext _context = null!;
        protected IPage Page { get; private set; } = null!;

        public BaseParser(PlaywrightFixture fixture)
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
    }
}