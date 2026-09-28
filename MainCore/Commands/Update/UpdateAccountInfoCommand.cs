namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateAccountInfoCommand(
        IChromeBrowser browser,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private async ValueTask HandleAsync(Command command)
        {
            var accountId = command.AccountId;
            var gold = await InfoParser.GetGold(browser.CurrentPage);
            var silver = await InfoParser.GetSilver(browser.CurrentPage);
            var hasPlusAccount = await InfoParser.HasPlusAccount(browser.CurrentPage);

            using var context = contextFactory.CreateDbContext();
            var dbAccountInfo = context.AccountsInfo
               .FirstOrDefault(x => x.AccountId == accountId.Value);

            if (dbAccountInfo is null)
            {
                var accountInfo = new AccountInfo
                {
                    AccountId = accountId.Value,
                    Gold = gold,
                    Silver = silver,
                    HasPlusAccount = hasPlusAccount,
                    Tribe = TribeEnums.Any,
                };
                context.Add(accountInfo);
            }
            else
            {
                dbAccountInfo.Gold = gold;
                dbAccountInfo.Silver = silver;
                dbAccountInfo.HasPlusAccount = hasPlusAccount;
                context.Update(dbAccountInfo);
            }
            context.SaveChanges();
        }
    }
}