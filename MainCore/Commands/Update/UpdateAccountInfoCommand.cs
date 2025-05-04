using MainCore.Commands.Base;

namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateAccountInfoCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            IDbContextFactory<AppDbContext> contextFactory,
            AccountInfoUpdated.Handler accountInfoUpdated,
            CancellationToken cancellationToken)
        {
            var chromeBrowser = chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var dto = Get(html);
            UpdateToDatabase(command.AccountId, dto, contextFactory);

            await accountInfoUpdated.HandleAsync(new(command.AccountId), cancellationToken);
            return Result.Ok();
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

        private static void UpdateToDatabase(AccountId accountId, AccountInfoDto dto, IDbContextFactory<AppDbContext> contextFactory)
        {
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