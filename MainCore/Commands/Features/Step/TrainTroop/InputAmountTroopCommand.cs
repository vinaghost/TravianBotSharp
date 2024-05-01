namespace MainCore.Commands.Features.Step.TrainTroop
{
    public class InputAmountTroopCommand : ByAccountIdBase, ICommand
    {
        public TroopEnums Troop { get; }
        public int Amount { get; }

        public InputAmountTroopCommand(AccountId accountId, TroopEnums troop, int amount) : base(accountId)
        {
            Troop = troop;
            Amount = amount;
        }
    }

    [RegisterAsTransient]
    public class InputAmountTroopCommandHandler : ICommandHandler<InputAmountTroopCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public InputAmountTroopCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(InputAmountTroopCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var inputBox = _troopPageParser.GetInputBox(html, command.Troop);
            if (inputBox is null) return Retry.TextboxNotFound("troop amount input");
            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(inputBox.XPath), $"{command.Amount}");
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var trainButton = _troopPageParser.GetTrainButton(html);
            if (trainButton is null) return Retry.ButtonNotFound("train troop");

            result = await chromeBrowser.Click(By.XPath(trainButton.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}