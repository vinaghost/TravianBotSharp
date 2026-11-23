namespace MainCore.Features.SetHeroPoint;

[Handler]
public static partial class SetHeroPointCommand
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => $"{nameof(SetHeroPointCommand).Humanize()} [{Description}]";
        private static string Description => $"{string.Join(", ", Ratios)}";
    }

    private static readonly string[] NameInputs = [
        "power",
        "offBonus",
        "defBonus",
        "productionPoints",
    ];

    private static readonly int[] Ratios = [1, 0, 0, 3];

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = command.Browser;
        Result result;

        var availablePointsLocator = browser.CurrentPage.GetPointAvailable();
        var availablePointsText = await availablePointsLocator.InnerTextAsync();
        var availablePoints = int.TryParse(availablePointsText, out var parsedAvailable) ? parsedAvailable : 0;
        if (availablePoints <= 0) return Result.Ok();

        var points = DistributePoints(availablePoints);

        for (var i = 0; i < NameInputs.Length; i++)
        {
            if (points[i] == 0) continue;
            var input = browser.CurrentPage.GetHeroAttributeBox(NameInputs[i]);

            var currentValueText = await input.InputValueAsync();
            var currentValue = int.TryParse(currentValueText, out var parsedCurrent) ? parsedCurrent : 0;
            var desiredValue = currentValue + points[i];

            result = await browser.Input(input, desiredValue.ToString());
            if (result.IsFailed) return result;
        }

        var saveButton = browser.CurrentPage.GetHeroAttributeSaveButton();
        result = await browser.Click(saveButton);
        if (result.IsFailed) return result;

        return Result.Ok();
    }

    private static int[] DistributePoints(int availablePoints)
    {
        var totalRatio = Ratios.Sum();
        var points = new int[Ratios.Length];
        var distributed = 0;

        for (var i = 0; i < Ratios.Length; i++)
        {
            points[i] = availablePoints * Ratios[i] / totalRatio;
            distributed += points[i];
        }

        // integer division leftovers go to the last attribute that has a non-zero ratio
        points[Array.FindLastIndex(Ratios, r => r > 0)] += availablePoints - distributed;
        return points;
    }
}