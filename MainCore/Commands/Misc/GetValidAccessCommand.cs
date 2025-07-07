using System.Net;

namespace MainCore.Commands.Misc
{
    [Handler]
    public static partial class GetValidAccessCommand
    {
        public sealed record Command(AccountId AccountId, bool IgnoreSleepTime = false) : IAccountCommand;

        private static async ValueTask<Result<AccessDto>> HandleAsync(
            Command command,
            ILogger logger,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            var (accountId, ignoreSleepTime) = command;

            var accesses = context.Accesses
               .Where(x => x.AccountId == accountId.Value)
               .OrderBy(x => x.LastUsed) // get oldest one
               .ToDto()
               .ToList();

            async Task<AccessDto?> GetValidAccess(List<AccessDto> proxies)
            {
                foreach (var proxy in proxies)
                {
                    var client = GetHttpClient(proxy);
                    logger.Information("Checking proxy {Proxy}, last used {LastUsed}", proxy.Proxy, proxy.LastUsed);
                    try
                    {
                        var response = await client.GetAsync(TRAVIAN_PAGE);
                        logger.Information("Access {Proxy} is good", proxy.Proxy);
                        return proxy;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "{message}", ex.Message);
                        return null;
                    }
                }
                return null;
            }

            var access = await GetValidAccess(accesses);
            if (access is null) return Stop.AllAccessNotWorking;

            if (accesses.Count == 1) return access;
            if (ignoreSleepTime) return access;

            var minSleep = context.ByName(accountId, AccountSettingEnums.SleepTimeMin);
            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return Stop.LackOfAccess;
            return access;
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