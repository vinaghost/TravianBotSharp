namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateAccountInfoCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context
            )
        {
            var dto = await Get(browser.CurrentPage);
            context.UpdateToDatabase(command.AccountId, dto);
        }

        private static async Task<AccountInfoDto> Get(IPage page)
        {
            var gold = await InfoParser.GetGold(page);
            var silver = await InfoParser.GetSilver(page);
            var hasPlusAccount = await InfoParser.HasPlusAccount(page);

            return new AccountInfoDto
            {
                Gold = gold,
                Silver = silver,
                HasPlusAccount = hasPlusAccount,
                Tribe = TribeEnums.Any,
            };
        }

        private static void UpdateToDatabase(this AppDbContext context, AccountId accountId, AccountInfoDto dto)
        {
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