using MainCore.Commands.Base;
using MainCore.Common.MediatR;

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

        private readonly IAccountInfoParser _accountInfoParser;
        private readonly IAccountInfoRepository _accountInfoRepository;

        public UpdateAccountInfoCommandHandler(IChromeManager chromeManager, IMediator mediator, IAccountInfoParser accountInfoParser, IAccountInfoRepository accountInfoRepository)
        {
            _chromeManager = chromeManager;
            _mediator = mediator;
            _accountInfoParser = accountInfoParser;
            _accountInfoRepository = accountInfoRepository;
        }

        public async Task<Result> Handle(UpdateAccountInfoCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dto = _accountInfoParser.Get(html);
            _accountInfoRepository.Update(command.AccountId, dto);

            await _mediator.Publish(new AccountInfoUpdated(command.AccountId), cancellationToken);

            return Result.Ok();
        }
    }
}