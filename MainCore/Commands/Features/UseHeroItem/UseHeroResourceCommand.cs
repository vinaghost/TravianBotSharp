namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public static partial class UseHeroResourceCommand
    {
        public sealed record Command(IChromeBrowser Browser, BuildingEnums Building, long[] Resource) : ICommand
        {
            public string Name => $"{nameof(UseHeroResourceCommand).Humanize()} {Description}";
            private string Description => $"{string.Join(", ", Resource.Select(x => $"{x}"))}";
        }

        private static async ValueTask<Result> HandleAsync(Command command)
        {
            var (browser, building, resource) = command;

            Result result;
            var fillUpButton = await UpgradeParser.GetFillUpButton(browser.CurrentPage, building);
            result = await browser.Click(fillUpButton);
            if (result.IsFailed) return result;

            result = await browser.Wait(InventoryParser.GetResourceTransferDialog(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await IsEnoughResource(browser, resource);
            if (result.IsFailed)
            {
                await browser.Refresh();
                return result;
            }

            result = await browser.Click(InventoryParser.GetResourceConfirmButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("&reload=auto");
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged(@"^(?!.*reload=auto).*");
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static readonly List<string> _itemInputName =
        [
            "lumber",
            "clay",
            "iron",
            "crop",
        ];

        private static async Task<Result> IsEnoughResource(IChromeBrowser browser, long[] requiredResources)
        {
            var errors = new List<Error>();
            var resources = await InventoryParser.GetInventoryResources(browser.CurrentPage);
            for (var i = 0; i < 4; i++)
            {
                if (resources[i] < requiredResources[i])
                {
                    errors.Add(MissingResource.Error($"{_itemInputName[i]}", resources[i], requiredResources[i]));
                }
            }
            return Result.FailIfNotEmpty(errors);
        }
    }
}