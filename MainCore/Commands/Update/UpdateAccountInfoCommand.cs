namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateAccountInfoCommand(
        ChromeManager chromeManager,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => "";
        }

        private async ValueTask HandleAsync(Command command)
        {
            var accountId = command.AccountId;
            var browser = chromeManager.Get(accountId);
            var dto = await InfoParser.GetAccountInfo(browser.CurrentPage);
            using var context = contextFactory.CreateDbContext();
            var dbAccountInfo = context.AccountsInfo
               .FirstOrDefault(x => x.AccountId == accountId.Value);

            if (dbAccountInfo is null)
            {
                var accountInfo = dto.ToEntity(accountId);
                context.Add(accountInfo);
            }
            else
            {
                dto.To(dbAccountInfo);
                context.Update(dbAccountInfo);
            }
            context.SaveChanges();
        }
    }
}