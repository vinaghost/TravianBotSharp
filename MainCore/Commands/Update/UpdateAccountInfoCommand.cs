namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateAccountInfoCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var html = browser.Html;

            var dto = Get(html);
            context.UpdateToDatabase(command.AccountId, dto);
        }

        private static AccountInfoDto Get(HtmlDocument doc)
        {
            var gold = InfoParser.GetGold(doc);
            var silver = InfoParser.GetSilver(doc);
            var hasPlusAccount = InfoParser.HasPlusAccount(doc);

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