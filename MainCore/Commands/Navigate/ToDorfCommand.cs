namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class ToDorfCommand
    {
        public sealed record Command(int Dorf) : ICommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IBrowser browser,
           CancellationToken cancellationToken
           )
        {
            var dorf = command.Dorf;

            var currentUrl = browser.CurrentUrl;
            var currentDorf = GetCurrentDorf(currentUrl);
            if (dorf == 0)
            {
                if (currentDorf == 0) dorf = 1;
                else dorf = currentDorf;
            }

            if (currentDorf != 0 && dorf == currentDorf)
            {
                return Result.Ok();
            }

            // Navigation bar bulunamadığında güçlü bekleme yap
            var button = NavigationBarParser.GetDorfButton(browser.Html, dorf);
            if (button is null)
            {
                // Önce kısa bekleme - sayfa yüklenmesi için
                await Task.Delay(2000, cancellationToken);
                
                button = NavigationBarParser.GetDorfButton(browser.Html, dorf);
                if (button is not null) 
                {
                    // Button bulundu, devam et
                }
                else
                {
                    // Hala bulunamıyorsa sayfayı yenile
                    var refreshResult = await browser.Refresh(cancellationToken);
                    if (refreshResult.IsFailed) return refreshResult;

                    // Sayfa yenilendikten sonra daha uzun bekle
                    await Task.Delay(5000, cancellationToken);

                    // Son bir kez daha dene
                    button = NavigationBarParser.GetDorfButton(browser.Html, dorf);
                    if (button is null) 
                    {
                        // Hala bulunamıyorsa, direkt URL ile git
                        var browserUrl = browser.CurrentUrl;
                        var targetUrl = dorf switch
                        {
                            1 => browserUrl.Replace("dorf2", "dorf1"),
                            2 => browserUrl.Replace("dorf1", "dorf2"),
                            _ => browserUrl
                        };
                        
                        if (targetUrl != browserUrl)
                        {
                            var navigateResult = await browser.Navigate(targetUrl, cancellationToken);
                            if (navigateResult.IsFailed) return navigateResult;
                            
                            await Task.Delay(2000, cancellationToken);
                            button = NavigationBarParser.GetDorfButton(browser.Html, dorf);
                        }
                        
                        if (button is null) return Retry.ButtonNotFound($"dorf{dorf}");
                    }
                }
            }

            Result result;
            result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;
            result = await browser.WaitPageChanged($"dorf{dorf}", cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static int GetCurrentDorf(string url)
        {
            if (url.Contains("dorf1")) return 1;
            if (url.Contains("dorf2")) return 2;
            return 0;
        }
    }
}
