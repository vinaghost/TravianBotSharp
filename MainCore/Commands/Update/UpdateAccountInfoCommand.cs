namespace MainCore.Commands.Update
{
    public class UpdateAccountInfoCommand : ByAccountIdBase, ICommand
    {
        public UpdateAccountInfoCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class UpdateAccountInfoCommandHandler : ICommandHandler<UpdateAccountInfoCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly IMediator _mediator;

        private readonly IAccountInfoRepository _accountInfoRepository;

        public UpdateAccountInfoCommandHandler(IChromeManager chromeManager, IMediator mediator, IAccountInfoRepository accountInfoRepository)
        {
            _chromeManager = chromeManager;
            _mediator = mediator;
            _accountInfoRepository = accountInfoRepository;
        }

        public async Task<Result> Handle(UpdateAccountInfoCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dto = Get(html);
            _accountInfoRepository.Update(command.AccountId, dto);

            await _mediator.Publish(new AccountInfoUpdated(command.AccountId), cancellationToken);

            return Result.Ok();
        }

        private static AccountInfoDto Get(HtmlDocument doc)
        {
            var dto = new AccountInfoDto()
            {
                Gold = GetGold(doc),
                Silver = GetSilver(doc),
                HasPlusAccount = HasPlusAccount(doc),
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
    }
}