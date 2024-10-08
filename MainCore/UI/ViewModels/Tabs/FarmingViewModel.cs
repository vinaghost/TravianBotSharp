using FluentValidation;
using MainCore.Commands.UI.Tabs;
using MainCore.Tasks;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Drawing;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<FarmingViewModel>]
    public class FarmingViewModel : AccountTabViewModelBase
    {
        public FarmListSettingInput FarmListSettingInput { get; } = new();
        public ListBoxItemViewModel FarmLists { get; } = new();

        private readonly IMediator _mediator;
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;

        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ReactiveCommand<Unit, Unit> UpdateFarmList { get; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> ActiveFarmList { get; }
        public ReactiveCommand<Unit, Unit> Start { get; }
        public ReactiveCommand<Unit, Unit> Stop { get; }
        public ReactiveCommand<AccountId, List<ListBoxItem>> LoadFarmList { get; }
        public ReactiveCommand<AccountId, Dictionary<AccountSettingEnums, int>> LoadSetting { get; }

        private static readonly Dictionary<Color, string> _activeTexts = new()
        {
            { Color.Green , "Deactive" },
            { Color.Red , "Active" },
            { Color.Black , "No farmlist selected" },
        };

        public FarmingViewModel(IMediator mediator, IDialogService dialogService, ITaskManager taskManager, IDbContextFactory<AppDbContext> contextFactory)
        {
            _mediator = mediator;
            _dialogService = dialogService;
            _taskManager = taskManager;
            _contextFactory = contextFactory;

            UpdateFarmList = ReactiveCommand.CreateFromTask(UpdateFarmListHandler);
            Start = ReactiveCommand.CreateFromTask(StartHandler);
            Stop = ReactiveCommand.CreateFromTask(StopHandler);

            Save = ReactiveCommand.CreateFromTask(SaveHandler);
            ActiveFarmList = ReactiveCommand.CreateFromTask(ActiveFarmListHandler);

            LoadFarmList = ReactiveCommand.Create<AccountId, List<ListBoxItem>>(LoadFarmListHandler);
            LoadSetting = ReactiveCommand.Create<AccountId, Dictionary<AccountSettingEnums, int>>(LoadSettingHandler);

            LoadFarmList.Subscribe(items =>
            {
                FarmLists.Load(items);
                if (items.Count > 0)
                {
                    var color = FarmLists.SelectedItem?.Color ?? Color.Black;
                    ActiveText = _activeTexts[color];
                }
            });
            LoadSetting.Subscribe(FarmListSettingInput.Set);
            ActiveFarmList.Subscribe(x =>
            {
                var color = FarmLists.SelectedItem?.Color ?? Color.Black;
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
            await LoadFarmList.Execute(accountId);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadFarmList.Execute(accountId);
            await LoadSetting.Execute(accountId);
        }

        private async Task UpdateFarmListHandler()
        {
            await _taskManager.AddOrUpdate<UpdateFarmListTask>(AccountId);
            _dialogService.ShowMessageBox("Information", "Added update farm list task");
        }

        private async Task StartHandler()
        {
            var getSetting = Locator.Current.GetService<IGetSetting>();
            var useStartAllButton = getSetting.BooleanByName(AccountId, AccountSettingEnums.UseStartAllButton);
            if (!useStartAllButton)
            {
                var count = CountActive(AccountId);
                if (count == 0)
                {
                    _dialogService.ShowMessageBox("Information", "There is no active farm or use start all button is disable");
                    return;
                }
            }
            await _taskManager.AddOrUpdate<StartFarmListTask>(AccountId);
            _dialogService.ShowMessageBox("Information", "Added start farm list task");
        }

        private async Task StopHandler()
        {
            var task = _taskManager.Get<StartFarmListTask>(AccountId);

            if (task is not null) await _taskManager.Remove(AccountId, task);

            _dialogService.ShowMessageBox("Information", "Removed start farm list task");
        }

        private async Task SaveHandler()
        {
            var saveSettingCommand = Locator.Current.GetService<SaveSettingCommand>();
            await saveSettingCommand.Execute(AccountId, FarmListSettingInput, CancellationToken.None);
        }

        private async Task ActiveFarmListHandler()
        {
            var selectedFarmList = FarmLists.SelectedItem;
            if (selectedFarmList is null)
            {
                _dialogService.ShowMessageBox("Warning", "No farm list selected");
                return;
            }

            UpdateFarm(new FarmId(selectedFarmList.Id));

            await _mediator.Publish(new FarmListUpdated(AccountId));
        }

        private static Dictionary<AccountSettingEnums, int> LoadSettingHandler(AccountId accountId)
        {
            var getSetting = Locator.Current.GetService<IGetSetting>();
            var items = getSetting.Get(accountId);
            return items;
        }

        private List<ListBoxItem> LoadFarmListHandler(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var items = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Color = x.IsActive ? Color.Green : Color.Red,
                    Content = x.Name,
                })
                .ToList();

            return items;
        }

        private string _activeText = "No farmlist selected";

        public string ActiveText
        {
            get => _activeText;
            set => this.RaiseAndSetIfChanged(ref _activeText, value);
        }

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