using MainCore.Constraints;

namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateAccountInfoCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            AccountInfoUpdated.Handler accountInfoUpdated,
            CancellationToken cancellationToken)
        {
            
            var html = browser.Html;

            var dto = Get(html);
            UpdateToDatabase(command.AccountId, dto, context);

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

        private static void UpdateToDatabase(AccountId accountId, AccountInfoDto dto, AppDbContext context)
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