namespace MainCore.Commands.Update
{
    public class UpdateAccountInfoCommand(IDbContextFactory<AppDbContext> contextFactory = null, IMediator mediator = null)
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        private readonly IMediator _mediator = mediator ?? Locator.Current.GetService<IMediator>();

        public async Task Execute(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var dto = Get(html);
            UpdateToDatabase(accountId, dto);

            await _mediator.Publish(new AccountInfoUpdated(accountId), cancellationToken);
        }

        private static AccountInfoDto Get(HtmlDocument doc)
        {
            var gold = GetGold(doc);
            var silver = GetSilver(doc);
            var hasPlusAccount = HasPlusAccount(doc);

            var dto = new AccountInfoDto()
            {
                Gold = gold,
                Silver = silver,
                HasPlusAccount = hasPlusAccount,
                Tribe = TribeEnums.Any,
            };
            return dto;
        }

        private static int GetGold(HtmlDocument doc)
        {
            var goldNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("ajaxReplaceableGoldAmount"));
            if (goldNode is null) return -1;
            return goldNode.InnerText.ParseInt();
        }

        private static int GetSilver(HtmlDocument doc)
        {
            var silverNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("ajaxReplaceableSilverAmount"));
            if (silverNode is null) return -1;
            return silverNode.InnerText.ParseInt();
        }

        private static bool HasPlusAccount(HtmlDocument doc)
        {
            var market = doc.DocumentNode.Descendants("a").FirstOrDefault(x => x.HasClass("market") && x.HasClass("round"));
            if (market is null) return false;

            if (market.HasClass("green")) return true;
            if (market.HasClass("gold")) return false;
            return false;
        }

        private void UpdateToDatabase(AccountId accountId, AccountInfoDto dto)
        {
            using var context = _contextFactory.CreateDbContext();

            var dbAccountInfo = context.AccountsInfo
                .Where(x => x.AccountId == accountId.Value)
                .FirstOrDefault();

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