using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Test
{
    public sealed class PlaywrightFixture : IAsyncLifetime
    {
        public IPlaywright PlaywrightInstance { get; private set; } = null!;
        public IBrowser Browser { get; private set; } = null!;

        public async ValueTask InitializeAsync()
        {
            PlaywrightInstance = await Playwright.CreateAsync();
            Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }

        public async ValueTask DisposeAsync()
        {
            await Browser.DisposeAsync();
            PlaywrightInstance.Dispose();
        }
    }
}