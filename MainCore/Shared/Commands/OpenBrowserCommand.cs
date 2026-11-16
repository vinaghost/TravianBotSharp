using System.Net;

namespace MainCore.Shared.Commands;

[Handler]
public sealed partial class OpenBrowserCommand(
    IDbContextFactory<AppDbContext> contextFactory,
    ILogger logger)
{
    public sealed record Command(IChromeBrowser Browser, bool IgnoreSleepTime = false) : IBrowserCommand
    {
        public string Name => $"{nameof(OpenBrowserCommand)}";
    }

    private async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, ignoreSleepTime) = command;
        var accountId = browser.AccountId;

        var result = await GetValidAccess(accountId, ignoreSleepTime);
        if (result.IsFailed) return Result.Fail(result.Errors);

        var access = result.Value;

        var (username, server, headless) = GetAccount(accountId);
        var chromeSetting = new ChromeSetting()
        {
            ProfilePath = GetProfilePath(server, username),
            ProxyHost = access.ProxyHost,
            ProxyPort = access.ProxyPort,
            ProxyUsername = access.ProxyUsername,
            ProxyPassword = access.ProxyPassword,
            IsHeadless = headless,
        };

        await browser.Setup(chromeSetting);
        await browser.Navigate($"{server}");

        UpdateLastUsed(access);
        return Result.Ok();
    }

    private static string GetProfilePath(string server, string username)
    {
        var uri = new Uri(server);
        var serverFolderName = uri.Host.Replace(".", "_");
        return Path.Combine(serverFolderName, username);
    }

    private (string Username, string Server, bool headless) GetAccount(AccountId accountId)
    {
        using var context = contextFactory.CreateDbContext();
        var account = context.Accounts
           .Where(x => x.Id == accountId.Value)
           .Select(x => new
           {
               x.Username,
               x.Server
           })
           .First();

        var headless = context.BooleanByName(accountId, AccountSettingEnums.HeadlessChrome);
        return (account.Username, account.Server, headless);
    }

    private void UpdateLastUsed(AccessDto access)
    {
        using var context = contextFactory.CreateDbContext();
        context.Accesses
           .Where(x => x.Id == access.Id.Value)
           .ExecuteUpdate(x => x.SetProperty(x => x.LastUsed, x => DateTime.Now));
    }

    private List<AccessDto> GetAccesses(AccountId accountId)
    {
        using var context = contextFactory.CreateDbContext();
        var accesses = context.Accesses
          .Where(x => x.AccountId == accountId.Value)
          .OrderBy(x => x.LastUsed)
          .Select(x => new AccessDto
          {
              Id = new AccessId(x.Id),
              Username = x.Username,
              Password = x.Password,
              ProxyHost = x.ProxyHost,
              ProxyPort = x.ProxyPort,
              ProxyUsername = x.ProxyUsername,
              ProxyPassword = x.ProxyPassword,
              LastUsed = x.LastUsed
          })
          .ToList();
        return accesses;
    }

    private int GetSleepTime(AccountId accountId)
    {
        using var context = contextFactory.CreateDbContext();
        return context.ByName(accountId, AccountSettingEnums.SleepTimeMin);
    }

    private async Task<Result<AccessDto>> GetValidAccess(AccountId accountId, bool ignoreSleepTime)
    {
        var accesses = GetAccesses(accountId);

        var access = await GetValidAccess(accesses);
        if (access is null)
        {
            logger.Error("All accesses not working");
            return Stop.Error;
        }

        if (accesses.Count == 1) return access;
        if (ignoreSleepTime) return access;

        var minSleep = GetSleepTime(accountId);
        var timeValid = DateTime.Now.AddMinutes(-minSleep);
        if (access.LastUsed > timeValid)
        {
            logger.Error("Last access is reused, it may get MH's attention");
            return Stop.Error;
        }

        logger.Information("Using connection {Proxy} to start chrome", access);
        return access;
    }

    private async Task<AccessDto?> GetValidAccess(List<AccessDto> proxies)
    {
        foreach (var proxy in proxies)
        {
            var client = GetHttpClient(proxy);
            logger.Information("Checking proxy {Proxy}, last used {LastUsed}", proxy, proxy.LastUsed);
            try
            {
                var response = await client.GetAsync(TRAVIAN_PAGE);
                if (response.IsSuccessStatusCode)
                {
                    logger.Information("Access {Proxy} is good", proxy);
                    return proxy;
                }

                logger.Warning("Access {Proxy} is not working, status code: {StatusCode}", proxy, response.StatusCode);
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

#pragma warning disable S5332 // Clear-text protocols should not be used
        var proxyUrl = $"http://{access.ProxyHost}:{access.ProxyPort}";
#pragma warning restore S5332 // Clear-text protocols should not be used

        if (string.IsNullOrEmpty(access.ProxyUsername))
        {
            _proxyWithoutAuth.Address = new Uri(proxyUrl);
            return _proxyWithoutAuthHttpClient;
        }

        _networkCredential.UserName = access.ProxyUsername;
        _networkCredential.Password = access.ProxyPassword;
        _proxyWithAuth.Address = new Uri(proxyUrl);
        return _proxyWithAuthHttpClient;
    }
}