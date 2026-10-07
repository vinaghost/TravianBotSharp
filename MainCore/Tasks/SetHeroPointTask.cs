using MainCore.Commands.Navigate;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class SetHeroPointTask(
        IChromeBrowser browser,
        SwitchTabCommand.Handler switchTabCommand)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Set hero point";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(AccountId, AccountSettingEnums.EnableAutoSetHeroPoint);
                if (!settingEnable) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(
#pragma warning disable S1172 // Unused method parameters should be removed
#pragma warning disable IDE0060 // Remove unused parameter
            Task task
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore S1172 // Unused method parameters should be removed
           )
        {
            Result result;
            var canSetHeroPoint = await browser.CurrentPage.Locator("#topBarHero i.levelUp.show").CountAsync() > 0;
            if (!canSetHeroPoint) return Result.Ok();
            result = await ToHeroPage();
            if (result.IsFailed) return result;
            result = await SetPoints();
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async ValueTask<Result> ToHeroPage()
        {
            var heroAvatar = browser.CurrentPage.Locator("#topBarHero a.#heroImageButton");
            var result = await browser.Click(heroAvatar);
            if (result.IsFailed) return result;

            result = await switchTabCommand.HandleAsync(new(1));
            if (result.IsFailed) return result;

            var attributeBox = browser.CurrentPage.Locator("#heroV2 .attributeBox .heroAttributes .attributes.formV2");
            result = await browser.Wait(attributeBox);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static readonly string[] NameInputs = [
            "power",
            "offBonus",
            "defBonus",
            "productionPoints",
            ];

        private async ValueTask<Result> SetPoints()
        {
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