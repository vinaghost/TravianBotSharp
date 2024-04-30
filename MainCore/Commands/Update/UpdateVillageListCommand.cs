using MainCore.Commands.Base;
using MainCore.Common.MediatR;

namespace MainCore.Commands.Update
{
    public class UpdateVillageListCommand : ByAccountIdBase, ICommand
    {
        public UpdateVillageListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class UpdateVillageListCommandHandler : ICommandHandler<UpdateVillageListCommand>
    {
        private readonly IMediator _mediator;
        private readonly IChromeManager _chromeManager;
        private readonly IVillagePanelParser _villagePanelParser;
        private readonly IVillageRepository _villageRepository;

        public UpdateVillageListCommandHandler(IMediator mediator, IChromeManager chromeManager, IVillagePanelParser villagePanelParser, IVillageRepository villageRepository)
        {
            _mediator = mediator;
            _chromeManager = chromeManager;
            _villagePanelParser = villagePanelParser;
            _villageRepository = villageRepository;
        }

        public async Task<Result> Handle(UpdateVillageListCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;
            var dtos = _villagePanelParser.Get(html);
            if (!dtos.Any()) return Retry.VillageListEmpty();

            _villageRepository.Update(accountId, dtos.ToList());
            await _mediator.Publish(new VillageUpdated(accountId), cancellationToken);
            return Result.Ok();
        }
    }
}