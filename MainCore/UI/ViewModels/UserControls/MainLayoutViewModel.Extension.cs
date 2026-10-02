using MainCore.Infrasturecture.Extensions;
using System.Net;

namespace MainCore.UI.ViewModels.UserControls
{
    public static class MainLayoutViewModelExtension
    {
        public static async Task<Result<AccessDto>> GetValidAccess(this AppDbContext context, AccountId accountId, bool ignoreSleepTime = false)
        {
            var accesses = context.Accesses
              .Where(x => x.AccountId == accountId.Value)
              .OrderBy(x => x.LastUsed) // get oldest one
              .ToDto()
              .ToList();

            var logger = context.GetAccountLogger(accountId);

            var access = await GetValidAccess(accesses, logger);
            if (access is null) return Stop.Error.WithError("All accesses not working");

            if (accesses.Count == 1) return access;
            if (ignoreSleepTime) return access;

            var minSleep = context.ByName(accountId, AccountSettingEnums.SleepTimeMin);
            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return Stop.Error.WithError("Last access is reused, it may get MH's attention");

            logger.Information("Using connection {Proxy} to start chrome", access.Proxy);
            return access;
        }

        private async static Task<AccessDto?> GetValidAccess(List<AccessDto> proxies, ILogger logger)
        {
            foreach (var proxy in proxies)
            {
                var client = GetHttpClient(proxy);
                logger.Information("Checking proxy {Proxy}, last used {LastUsed}", proxy.Proxy, proxy.LastUsed);
                try
                {
                    var response = await client.GetAsync(TRAVIAN_PAGE);
                    if (response.IsSuccessStatusCode)
                    {
                        logger.Information("Access {Proxy} is good", proxy.Proxy);
                        return proxy;
                    }

                    logger.Warning("Access {Proxy} is not working, status code: {StatusCode}", proxy.Proxy, response.StatusCode);
                    continue;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "{Message}", ex.Message);
                }
            }
            return null;
        }

        private static readonly NetworkCredential _networkCredential = new();

        private static readonly WebProxy _proxyWithAuth = new()
        {
            Credentials = _networkCredential,
        };

        private static readonly WebProxy _proxyWithoutAuth = new();

        private static readonly HttpClient _proxyWithoutAuthHttpClient = new(new HttpClientHandler()
        {
            Proxy = _proxyWithoutAuth,
            UseProxy = true,
        });

        private static readonly HttpClient _proxyWithAuthHttpClient = new(new HttpClientHandler()
        {
            Proxy = _proxyWithAuth,
            UseProxy = true,
        });

        private static readonly HttpClient _defaultHttpClient = new(new HttpClientHandler()
        {
            UseProxy = false,
        });

        private const string TRAVIAN_PAGE = "https://www.travian.com/international";

        private static HttpClient GetHttpClient(AccessDto access)
        {
            if (string.IsNullOrEmpty(access.ProxyHost)) return _defaultHttpClient;

            if (string.IsNullOrEmpty(access.ProxyUsername))
            {
                _proxyWithoutAuth.Address = new Uri($"http://{access.ProxyHost}:{access.ProxyPort}");
                return _proxyWithoutAuthHttpClient;
            }

            _networkCredential.UserName = access.ProxyUsername;
            _networkCredential.Password = access.ProxyPassword;
            _proxyWithAuth.Address = new Uri($"http://{access.ProxyHost}:{access.ProxyPort}");
            return _proxyWithAuthHttpClient;
        }
    }
}