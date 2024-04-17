using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Update
{
    public class UpdateAccountInfoCommand : ByAccountIdBase, ICommand
    {
        public UpdateAccountInfoCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateAccountInfoCommandHandler : ICommandHandler<UpdateAccountInfoCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly IMediator _mediator;

        private readonly IAccountInfoParser _accountInfoParser;
        private readonly IHeroParser _heroParser;
        private readonly IAccountInfoRepository _accountInfoRepository;

        public UpdateAccountInfoCommandHandler(IChromeManager chromeManager, IMediator mediator, IAccountInfoParser accountInfoParser, IHeroParser heroParser, IAccountInfoRepository accountInfoRepository)
        {
            _chromeManager = chromeManager;
            _mediator = mediator;
            _accountInfoParser = accountInfoParser;
            _heroParser = heroParser;
            _accountInfoRepository = accountInfoRepository;
        }

        public async Task<Result> Handle(UpdateAccountInfoCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dto = _accountInfoParser.Get(html);
            _accountInfoRepository.Update(command.AccountId, dto);

            await _mediator.Publish(new AccountInfoUpdated(command.AccountId), cancellationToken);

            if (_heroParser.CanStartAdventure(html))
            {
                await _mediator.Publish(new AdventureUpdated(command.AccountId), cancellationToken);
            }

            return Result.Ok();
        }
    }
}