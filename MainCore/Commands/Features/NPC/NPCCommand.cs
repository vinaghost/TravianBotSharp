namespace MainCore.Commands.Features.NPC
{
    [Handler]
    public static partial class NPCCommand
    {
        public sealed record Command(IChromeBrowser Browser, long[] Values);

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var (browser, values) = command;
            Result result;

            result = await InputAmount(browser, values);
            if (result.IsFailed) return result;

            result = await browser.Click(NpcResourceParser.GetDistributeButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(NpcResourceParser.GetRedeemButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async ValueTask<Result> InputAmount(IChromeBrowser browser, long[] values)
        {
            var inputs = NpcResourceParser.GetInputs(browser.CurrentPage);

            var inputCount = await inputs.CountAsync();

            if (inputCount != 4)
            {
                return Stop.Error.WithError($"Expected 4 input elements, but found {inputCount}.");
            }

            for (var i = 0; i < 4; i++)
            {
                var inputElement = inputs.Nth(i);
                var result = await browser.Input(inputElement, $"{values[i]}");
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }
    }
}