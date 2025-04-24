using FluentValidation;
using MainCore.Commands.UI;
using MainCore.Tasks;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<FarmingViewModel>]
    public partial class FarmingViewModel : AccountTabViewModelBase
    {
        public AccountSettingInput AccountSettingInput { get; } = new();
        public ListBoxItemViewModel FarmLists { get; } = new();

        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;

        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly FarmListUpdated.Handler _farmListUpdated;

        private static readonly Dictionary<SplatColor, string> _activeTexts = new()
        {
            { SplatColor.Green , "Deactive" },
            { SplatColor.Red , "Active" },
            { SplatColor.Black , "No farmlist selected" },
        };

        public FarmingViewModel(IDialogService dialogService, ITaskManager taskManager, IDbContextFactory<AppDbContext> contextFactory, FarmListUpdated.Handler farmListUpdated)
        {
            _dialogService = dialogService;
            _taskManager = taskManager;
            _contextFactory = contextFactory;
            _farmListUpdated = farmListUpdated;

            LoadFarmListCommand.Subscribe(items =>
            {
                FarmLists.Load(items);
                if (items.Count > 0)
                {
                    var color = FarmLists.SelectedItem?.Color ?? SplatColor.Black;
                    ActiveText = _activeTexts[color];
                }
            });
            LoadSettingCommand.Subscribe(AccountSettingInput.Set);
            ActiveFarmListCommand.Subscribe(x =>
            {
                var color = FarmLists.SelectedItem?.Color ?? SplatColor.Black;
                ActiveText = _activeTexts[color];
            });

            this.WhenAnyValue(x => x.FarmLists.SelectedItem)
                .WhereNotNull()
                .Subscribe(selectedItem =>
                {
                    var color = selectedItem.Color;
                    ActiveText = _activeTexts[color];
                });
        }

        public async Task FarmListRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadFarmListCommand.Execute(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadFarmListCommand.Execute(accountId);
            await LoadSettingCommand.Execute(accountId);
        }

        [ReactiveCommand]
        private async Task UpdateFarmList()
        {
            await _taskManager.AddOrUpdate<UpdateFarmListTask>(AccountId);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Added update farm list task"));
        }

        [ReactiveCommand]
        private async Task Start()
        {
            var getSetting = Locator.Current.GetService<IGetSetting>();
            var useStartAllButton = getSetting.BooleanByName(AccountId, AccountSettingEnums.UseStartAllButton);
            if (!useStartAllButton)
            {
                var count = CountActive(AccountId);
                if (count == 0)
                {
                    await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "There is no active farm or use start all button is disable"));
                    return;
                }
            }
            await _taskManager.AddOrUpdate<StartFarmListTask>(AccountId);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Added start farm list task"));
        }

        [ReactiveCommand]
        private async Task Stop()
        {
            var task = _taskManager.Get<StartFarmListTask>(AccountId);

            if (task is not null) await _taskManager.Remove(AccountId, task);

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Removed start farm list task"));
        }

        [ReactiveCommand]
        private async Task Save()
        {
            var saveSettingCommand = Locator.Current.GetService<SaveSettingCommand>();
            await saveSettingCommand.Execute(AccountId, AccountSettingInput, CancellationToken.None);
        }

        [ReactiveCommand]
        private async Task ActiveFarmList()
        {
            var selectedFarmList = FarmLists.SelectedItem;
            if (selectedFarmList is null)
            {
                await _dialogService.ConfirmBox.Handle(new MessageBoxData("Warning", "No farm list selected"));
                return;
            }

            UpdateFarm(new FarmId(selectedFarmList.Id));

            await _farmListUpdated.HandleAsync(new(AccountId));
        }

        [ReactiveCommand]
        private static Dictionary<AccountSettingEnums, int> LoadSetting(AccountId accountId)
        {
            var getSetting = Locator.Current.GetService<IGetSetting>();
            var items = getSetting.Get(accountId);
            return items;
        }

        [ReactiveCommand]
        private List<ListBoxItem> LoadFarmList(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var items = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Color = x.IsActive ? SplatColor.Green : SplatColor.Red,
                    Content = x.Name,
                })
                .ToList();

            return items;
        }

        [Reactive]
        private string _activeText = "No farmlist selected";

        private int CountActive(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var count = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.IsActive)
                .Count();
            return count;
        }

        private void UpdateFarm(FarmId farmId)
        {
            using var context = _contextFactory.CreateDbContext();
            context.FarmLists
               .Where(x => x.Id == farmId.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.IsActive, x => !x.IsActive));
        }
    }
}