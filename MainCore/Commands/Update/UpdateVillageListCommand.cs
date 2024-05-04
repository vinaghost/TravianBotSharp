using MainCore.Commands.Abstract;

namespace MainCore.Commands.Update
{
    public class UpdateVillageListCommand : ByAccountIdBase, ICommand
    {
        public IChromeBrowser ChromeBrowser { get; }

        public UpdateVillageListCommand(AccountId accountId, IChromeBrowser chromeBrowser) : base(accountId)
        {
            ChromeBrowser = chromeBrowser;
        }
    }

    public class UpdateVillageListCommandHandler : VillagePanelCommand, ICommandHandler<UpdateVillageListCommand>
    {
        private readonly IMediator _mediator;
        private readonly IVillageRepository _villageRepository;

        public UpdateVillageListCommandHandler(IMediator mediator, IVillageRepository villageRepository)
        {
            _mediator = mediator;
            _villageRepository = villageRepository;
        }

        public async Task<Result> Handle(UpdateVillageListCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var chromeBrowser = request.ChromeBrowser;
            var html = chromeBrowser.Html;
            var dtos = Get(html);
            if (!dtos.Any()) return Retry.VillageListEmpty();

            _villageRepository.Update(accountId, dtos.ToList());
            await _mediator.Publish(new VillageUpdated(accountId), cancellationToken);
            return Result.Ok();
        }
    }
}