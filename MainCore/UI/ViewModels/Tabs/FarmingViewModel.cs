using MainCore.Infrasturecture.Extensions;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

using Splat;

namespace MainCore.UI.ViewModels.Tabs
{
    using FluentValidation;

    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<FarmingViewModel>]
    public partial class FarmingViewModel : AccountTabViewModelBase
    {
        public AccountSettingInput AccountSettingInput { get; } = new();
        public ListBoxItemViewModel FarmLists { get; } = new();

        private readonly DialogService _dialogService;
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly TaskManager _taskManager;

        private static readonly Dictionary<SplatColor, string> _activeTexts = new()
        {
            { SplatColor.Green , "Deactive" },
            { SplatColor.Red , "Active" },
            { SplatColor.Black , "No farmlist selected" },
        };

        public FarmingViewModel(DialogService dialogService, IValidator<AccountSettingInput> accountsettingInputValidator, TaskManager taskManager, RxQueue rxQueue, IDbContextFactory<AppDbContext> contextFactory)
        {
            _accountsettingInputValidator = accountsettingInputValidator;
            _dialogService = dialogService;
            _taskManager = taskManager;
            _contextFactory = contextFactory;

            LoadFarmListCommand
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(items =>
            {
                FarmLists.Load(items);
                if (items.Count > 0)
                {
                    var color = FarmLists.SelectedItem?.Color ?? SplatColor.Black;
                    ActiveText = _activeTexts[color];
                }
            });
            LoadSettingCommand
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(AccountSettingInput.Set);

            ActiveFarmListCommand
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(x =>
            {
                var color = FarmLists.SelectedItem?.Color ?? SplatColor.Black;
                ActiveText = _activeTexts[color];
            });

            this.WhenAnyValue(x => x.FarmLists.SelectedItem)
                .WhereNotNull()
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(selectedItem =>
                {
                    var color = selectedItem.Color;
                    ActiveText = _activeTexts[color];
                });

            rxQueue.GetObservable<FarmsModified>()
                .InvokeCommand(FarmsModifiedCommand);

            FarmsModifiedCommand
                .Where(x => x)
                .Select(_ => AccountId)
                .Throttle(TimeSpan.FromMilliseconds(1000), RxSchedulers.TaskpoolScheduler)
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
                .InvokeCommand(LoadFarmListCommand);
        }

        [ReactiveCommand(RunInBackground = true)]
        public bool FarmsModified(FarmsModified notification)
        {
            if (!IsActive) return false;
            if (notification.AccountId != AccountId) return false;
            return true;
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadFarmListCommand.Execute(accountId);
            await LoadSettingCommand.Execute(accountId);
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task UpdateFarmList()
        {
            _taskManager.AddOrUpdate<UpdateFarmListTask.Task>(new(AccountId));
            await _dialogService.SendMessage("Information", "Added update farm list task");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Start()
        {
            using var context = _contextFactory.CreateDbContext();

            var useStartAllButton = context.BooleanByName(AccountId, AccountSettingEnums.UseStartAllButton);
            if (!useStartAllButton)
            {
                var count = context.FarmLists
                    .Where(x => x.AccountId == AccountId.Value)
                    .Count(x => x.IsActive);

                if (count == 0)
                {
                    await _dialogService.SendMessage("Information", "There is no active farm or use start all button is disable");
                    return;
                }
            }
            _taskManager.AddOrUpdate<StartFarmListTask.Task>(new(AccountId));
            await _dialogService.SendMessage("Information", "Added start farm list task");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Stop()
        {
            _taskManager.Remove<StartFarmListTask.Task>(AccountId);
            await _dialogService.SendMessage("Information", "Removed start farm list task");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task Save()
        {
            var result = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
                return;
            }

            using var context = _contextFactory.CreateDbContext();
            var settings = AccountSettingInput.Get();
            context.SaveAccountSetting(AccountId, settings);
            await _dialogService.SendMessage("Information", "Saved");
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task ActiveFarmList()
        {
            if (FarmLists.SelectedItem is null)
            {
                await _dialogService.SendMessage("Warning", "No farm list selected");
                return;
            }

            var selectedFarmList = FarmLists.SelectedItem;
            if (selectedFarmList is null) return;

            using var context = _contextFactory.CreateDbContext();
            context.FarmLists
               .Where(x => x.Id == selectedFarmList.Id)
               .ExecuteUpdate(x => x.SetProperty(x => x.IsActive, x => !x.IsActive));

            await FarmsModifiedCommand.Execute(new FarmsModified(AccountId));
            await _dialogService.SendMessage("Information", "Activated farm list");
        }

        [ReactiveCommand(RunInBackground = true)]
        private Dictionary<AccountSettingEnums, int> LoadSetting(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var settings = context.AccountsSetting
              .Where(x => x.AccountId == AccountId.Value)
              .ToDictionary(x => x.Setting, x => x.Value);
            return settings;
        }

        [ReactiveCommand(RunInBackground = true)]
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
    }
}