using MainCore.Commands.Abstract;
using System.Net;

namespace MainCore.Commands.Queries
{
    [RegisterSingleton<GetAccess>]
    public sealed class GetAccess : QueryBase, IDisposable
    {
        private readonly ILogService _logService;
        private readonly IGetSetting _getSetting;

        private readonly NetworkCredential _networkCredential;
        private readonly WebProxy _proxyWithAuth;
        private readonly WebProxy _proxyWithoutAuth;

        private readonly HttpClient _proxyWithoutAuthHttpClient;
        private readonly HttpClient _proxyWithAuthHttpClient;
        private readonly HttpClient _defaultHttpClient;

        private const string TRAVIAN_PAGE = "https://www.travian.com/international";

        public GetAccess(IDbContextFactory<AppDbContext> contextFactory, ILogService logService, IGetSetting getSetting) : base(contextFactory)
        {
            _logService = logService;
            _getSetting = getSetting;

            _networkCredential = new NetworkCredential();
            _proxyWithAuth = new WebProxy()
            {
                Credentials = _networkCredential,
            };
            _proxyWithoutAuth = new WebProxy();

            var proxyHttpClientHandler = new HttpClientHandler()
            {
                Proxy = _proxyWithoutAuth,
                UseProxy = true,
            };
            _proxyWithoutAuthHttpClient = new HttpClient(proxyHttpClientHandler);

            var proxyWithAuthHttpClientHandler = new HttpClientHandler()
            {
                Proxy = _proxyWithAuth,
                UseProxy = true,
            };
            _proxyWithAuthHttpClient = new HttpClient(proxyWithAuthHttpClientHandler);

            var defaultHttpClientHandler = new HttpClientHandler()
            {
                UseProxy = false,
            };

            _defaultHttpClient = new HttpClient(defaultHttpClientHandler);
        }

        public async Task<Result<AccessDto>> Execute(AccountId accountId, bool ignoreSleepTime = false)
        {
            var access = await GetValidAccess(accountId);
            if (access is null) return Stop.AllAccessNotWorking;

            var count = CountAccesses(accountId);
            if (count == 1) return access;
            if (ignoreSleepTime) return access;

            var minSleep = _getSetting.ByName(accountId, AccountSettingEnums.SleepTimeMin);
            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return Stop.LackOfAccess;
            return access;
        }

        private async Task<AccessDto> GetValidAccess(AccountId accountId)
        {
            var accesses = GetAccesses(accountId);
            var logger = _logService.GetLogger(accountId);

            foreach (var access in accesses)
            {
                if (await IsValid(access, logger))
                {
                    return access;
                }
            }

            return null;
        }

        private async Task<bool> IsValid(AccessDto access, ILogger logger)
        {
            var client = GetClient(access);
            logger.Information("Checking access {Proxy}, last used {LastUsed}", access.Proxy, access.LastUsed);
            try
            {
                var response = await client.GetAsync(TRAVIAN_PAGE);
                logger.Information("Access {Proxy} is good", access.Proxy);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There is exception");
                return false;
            }
        }

        private HttpClient GetClient(AccessDto access)
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

        private List<AccessDto> GetAccesses(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var accessess = context.Accesses
               .Where(x => x.AccountId == accountId.Value)
               .OrderBy(x => x.LastUsed) // get oldest one
               .ToDto()
               .ToList();
            return accessess;
        }

        private int CountAccesses(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Accesses
               .Where(x => x.AccountId == accountId.Value)
               .Count();
        }

        public void Dispose()
        {
            _proxyWithoutAuthHttpClient.Dispose();
            _proxyWithAuthHttpClient.Dispose();
            _defaultHttpClient.Dispose();
        }
    }
}