using MainCore.Constraints;
using System.Net;

namespace MainCore.Queries
{
    [Handler]
    public static partial class VerifyAccessQuery
    {
        public sealed record Query(AccountId AccountId, AccessDto AcccessDto) : IAccountQuery;

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

        private static async ValueTask<bool> HandleAsync(
            Query query,
            ILogger logger,
            CancellationToken cancellationToken
            )
        {
            var (accountId, access) = query;

            var client = GetHttpClient(access);
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