using MainCore.Commands.Abstract;

namespace MainCore.Commands.Update
{
    [RegisterScoped<UpdateAccountInfoCommand>]
    public class UpdateAccountInfoCommand(IDataService dataService, IDbContextFactory<AppDbContext> contextFactory, IMediator mediator) : CommandBase(dataService), ICommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
        private readonly IMediator _mediator = mediator;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var chromeBrowser = _dataService.ChromeBrowser;

            var html = chromeBrowser.Html;
            var dto = Get(html);
            UpdateToDatabase(accountId, dto);

            await _mediator.Publish(new AccountInfoUpdated(accountId), cancellationToken);
            return Result.Ok();
        }

        private static AccountInfoDto Get(HtmlDocument doc)
        {
            var gold = InfoParser.GetGold(doc);
            var silver = InfoParser.GetSilver(doc);
            var hasPlusAccount = InfoParser.HasPlusAccount(doc);

            var dto = new AccountInfoDto()
            {
                Gold = gold,
                Silver = silver,
                HasPlusAccount = hasPlusAccount,
                Tribe = TribeEnums.Any,
            };
            return dto;
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