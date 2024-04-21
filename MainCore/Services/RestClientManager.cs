using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Net;

namespace MainCore.Services
{
    [RegisterAsSingleton]
    public class RestClientManager : IRestClientManager
    {
        private readonly Dictionary<int, RestClient> _database = new();
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public RestClientManager(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public RestClient Get(AccountId accountId, AccessDto access)
        {
            if (_database.TryGetValue(access.Id.Value, out RestClient client))
            {
                return client;
            }

            IWebProxy proxy = null;
            if (!string.IsNullOrEmpty(access.ProxyHost))
            {
                if (!string.IsNullOrEmpty(access.ProxyUsername)) // Proxy auth

                {
                    ICredentials credentials = new NetworkCredential(access.ProxyUsername, access.ProxyPassword);

                    proxy = new WebProxy($"{access.ProxyHost}:{access.ProxyPort}", false, null, credentials);
                }
                else // Without proxy auth
                {
                    proxy = new WebProxy(access.ProxyHost, access.ProxyPort);
                }
            }

            using var context = _contextFactory.CreateDbContext();
            var url = context.Accounts
                .Where(x => x.Id == accountId.Value)
                .Select(x => $"{x.Server}dorf1.php")
                .FirstOrDefault();

            var clientOptions = new RestClientOptions
            {
                MaxTimeout = 30000,
                BaseUrl = new Uri(url),
                Proxy = proxy,
            };

            client = new RestClient(clientOptions);
            _database.Add(access.Id.Value, client);
            return client;
        }

        public void Shutdown()
        {
            foreach (var item in _database.Values)
            {
                item.Dispose();
            }
        }
    }
}