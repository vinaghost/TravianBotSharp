using MainCore.Commands.Base;
using MainCore.Common.MediatR;

namespace MainCore.Commands.Features.Step.TrainTroop
{
    public class GetMaximumTroopCommand : ByAccountIdBase, ICommand<int>
    {
        public TroopEnums Troop { get; }

        public GetMaximumTroopCommand(AccountId accountId, TroopEnums troop) : base(accountId)
        {
            Troop = troop;
        }
    }

    [RegisterAsTransient]
    public class GetMaximumTroopCommandHandler : ICommandHandler<GetMaximumTroopCommand, int>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public GetMaximumTroopCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public int Value { get; private set; }

        public async Task<Result> Handle(GetMaximumTroopCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            Value = _unitOfParser.TroopPageParser.GetMaxAmount(html, command.Troop);
            return Result.Ok();
        }
    }
}