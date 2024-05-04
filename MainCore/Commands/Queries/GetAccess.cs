using MainCore.Commands.Checks;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Commands.Queries
{
    public class GetAccess
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogService _logService;

        public GetAccess(IDbContextFactory<AppDbContext> contextFactory = null, ILogService logService = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
            _logService = logService ?? Locator.Current.GetService<ILogService>();
        }

        public async Task<Result<AccessDto>> Execute(AccountId accountId, bool ignoreSleepTime = false)
        {
            var accesses = GetAccesses(accountId);
            var logger = _logService.GetLogger(accountId);

            var access = await GetValidAccess(accesses, logger);

            if (access is null) return Stop.AllAccessNotWorking;

            UpdateAccessLastUsed(access.Id);

            if (accesses.Count == 1) return access;
            if (ignoreSleepTime) return access;

            var minSleep = new GetAccountSetting().ByName(accountId, AccountSettingEnums.SleepTimeMin);

            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return Stop.LackOfAccess;
            return access;
        }

        private static async Task<AccessDto> GetValidAccess(List<AccessDto> accesses, ILogger logger)
        {
            foreach (var access in accesses)
            {
                logger.Information("Check connection {proxy}", access.Proxy);
                var valid = await new CheckProxyCommand().Execute(access);

                if (!valid)
                {
                    logger.Warning("Connection {proxy} cannot connect to travian.com", access.Proxy);
                    continue;
                }

                logger.Information("Connection {proxy} is working", access.Proxy);
                return access;
            }
            return null;
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

        private void UpdateAccessLastUsed(AccessId accessId)
        {
            using var context = _contextFactory.CreateDbContext();
            var access = context.Accesses
               .Where(x => x.Id == accessId.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.LastUsed, x => DateTime.Now));
        }
    }
}