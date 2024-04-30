namespace MainCore.Commands.Update
{
    public class UpdateFarmListCommand : ByAccountIdBase, ICommand
    {
        public UpdateFarmListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateFarmListCommandHandler : UpdateCommandHandlerBase, ICommandHandler<UpdateFarmListCommand>
    {
        public UpdateFarmListCommandHandler(IChromeManager chromeManager, IMediator mediator, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task<Result> Handle(UpdateFarmListCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dtos = _unitOfParser.FarmParser.Get(html);
            _unitOfRepository.FarmRepository.Update(command.AccountId, dtos.ToList());
            await _mediator.Publish(new FarmListUpdated(command.AccountId), cancellationToken);
            return Result.Ok();
        }
    }
}