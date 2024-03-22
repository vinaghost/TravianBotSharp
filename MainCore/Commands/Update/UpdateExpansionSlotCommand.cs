using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Update
{
    public class UpdateExpansionSlotCommand : ByAccountVillageIdBase, ICommand
    {
        public UpdateExpansionSlotCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateExpansionSlotCommandHandler : UpdateCommandHandlerBase, ICommandHandler<UpdateExpansionSlotCommand>
    {
        public UpdateExpansionSlotCommandHandler(IChromeManager chromeManager, IMediator mediator, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task<Result> Handle(UpdateExpansionSlotCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dtos = _unitOfParser.SettleParser.Get(html);
            _unitOfRepository.ExpansionSlotRepository.Update(command.VillageId, dtos.ToList());
            return Result.Ok();
        }
    }
}