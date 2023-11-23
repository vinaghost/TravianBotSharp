using FluentResults;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Update
{
    [RegisterAsTransient]
    public class UpdateAccountInfoCommand : UpdateCommandBase, IUpdateAccountInfoCommand
    {
        public UpdateAccountInfoCommand(IChromeManager chromeManager, IMediator mediator, IUnitOfRepository unitOfRepository, IUnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task<Result> Execute(AccountId accountId)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;
            var dto = _unitOfParser.AccountInfoParser.Get(html);
            _unitOfRepository.AccountInfoRepository.Update(accountId, dto);

            await _mediator.Publish(new AccountInfoUpdated(accountId));
            return Result.Ok();
        }
    }
}