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
    public class UpdateVillageListCommand : ByAccountIdBase, ICommand
    {
        public UpdateVillageListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateVillageListCommandHandler : UpdateCommandHandlerBase, ICommandHandler<UpdateVillageListCommand>
    {
        public UpdateVillageListCommandHandler(IChromeManager chromeManager, IMediator mediator, UnitOfRepository unitOfRepository, IUnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task<Result> Handle(UpdateVillageListCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dtos = _unitOfParser.VillagePanelParser.Get(html);
            _unitOfRepository.VillageRepository.Update(command.AccountId, dtos.ToList());
            await _mediator.Publish(new VillageUpdated(command.AccountId), cancellationToken);
            return Result.Ok();
        }
    }
}