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
    public class UpdateFarmListCommand : ByAccountIdBase, IRequest
    {
        public UpdateFarmListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateFarmListCommandHandler : UpdateCommandHandlerBase, IRequestHandler<UpdateFarmListCommand>
    {
        public UpdateFarmListCommandHandler(IChromeManager chromeManager, IMediator mediator, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task Handle(UpdateFarmListCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dtos = _unitOfParser.FarmParser.Get(html);
            _unitOfRepository.FarmRepository.Update(command.AccountId, dtos.ToList());
            await _mediator.Publish(new FarmListUpdated(command.AccountId), cancellationToken);
        }
    }
}