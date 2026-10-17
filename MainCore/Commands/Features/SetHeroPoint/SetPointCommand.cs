using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Commands.Features.SetHeroPoint
{
    [Handler]
    public static partial class SetPointCommand
    {
        public sealed record Command(IChromeBrowser Browser);

        private static readonly string[] NameInputs = [
            "power",
            "offBonus",
            "defBonus",
            "productionPoints",
            ];

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;
            Result result;

            int[] Points = [1, 0, 0, 3];

            for (var i = 0; i < NameInputs.Length; i++)
            {
                if (Points[i] == 0) continue;
                var input = browser.CurrentPage.Locator($"#heroV2 .attributeBox .heroAttributes .attributes.formV2 input[name='{NameInputs[i]}']");

                var currentValueText = await input.InputValueAsync();
                var currentValue = int.TryParse(currentValueText, out var value) ? value : 0;
                var desiredValue = currentValue + Points[i];

                result = await browser.Input(input, desiredValue.ToString());
                if (result.IsFailed) return result;
            }

            var saveButton = browser.CurrentPage.Locator("#heroV2 .attributeBox .heroAttributes .attributes.formV2 button#savePoints");
            result = await browser.Click(saveButton);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}