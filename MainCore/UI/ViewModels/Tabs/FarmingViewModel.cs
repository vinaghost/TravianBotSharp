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
        public ListBoxItemViewModel CenterVillages { get; } = new();

        // a player whose population did not grow for this many days counts as inactive
        private const int InactiveDays = 2;

        private readonly DialogService _dialogService;
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly TaskManager _taskManager;
        private readonly MapDataService _mapDataService;

        private static readonly Dictionary<SplatColor, string> _activeTexts = new()
        {
            { SplatColor.Green , "Deactive" },
            { SplatColor.Red , "Active" },
            { SplatColor.Black , "No farmlist selected" },
        };

        public FarmingViewModel(DialogService dialogService, IValidator<AccountSettingInput> accountsettingInputValidator, TaskManager taskManager, RxQueue rxQueue, IDbContextFactory<AppDbContext> contextFactory, MapDataService mapDataService)
        {
            _mapDataService = mapDataService;
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

            LoadCenterVillagesCommand
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(items =>
                {
                    CenterVillages.Load(items);
                    if (CenterVillages.SelectedItem is null || !CenterVillages.Items.Contains(CenterVillages.SelectedItem))
                    {
                        CenterVillages.SelectedItem = CenterVillages.Items.FirstOrDefault();
                    }
                });

            FindInactivesCommand
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(output =>
                {
                    InactiveStatus = output.Status;
                    InactiveVillages = output.Villages;
                });

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
            await LoadCenterVillagesCommand.Execute(accountId);
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

        [ReactiveCommand(RunInBackground = true)]
        private List<ListBoxItem> LoadCenterVillages(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .OrderBy(x => x.Id)
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Content = x.Name + " (" + x.X + "|" + x.Y + ")",
                })
                .ToList();
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task<InactiveSearchOutput> FindInactives(CancellationToken cancellationToken)
        {
            var center = CenterVillages.SelectedItem;
            if (center is null)
            {
                return new("Select the village to search around", []);
            }

            var validation = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput, cancellationToken);
            if (!validation.IsValid)
            {
                return new(string.Join(Environment.NewLine, validation.Errors.Select(x => x.ErrorMessage)), []);
            }

            var radius = AccountSettingInput.InactiveFinderRadius.Get();
            var (minPopulation, maxPopulation) = AccountSettingInput.InactiveFinderPopulation.Get();
            var (minPlayerPopulation, maxPlayerPopulation) = AccountSettingInput.InactiveFinderPlayerPopulation.Get();

            string server;
            List<(int X, int Y)> ownVillages;
            (int X, int Y) centerCoordinates;
            using (var context = _contextFactory.CreateDbContext())
            {
                context.SaveAccountSetting(AccountId, new()
                {
                    { AccountSettingEnums.InactiveFinderRadius, radius },
                    { AccountSettingEnums.InactiveFinderMinPopulation, minPopulation },
                    { AccountSettingEnums.InactiveFinderMaxPopulation, maxPopulation },
                    { AccountSettingEnums.InactiveFinderMinPlayerPopulation, minPlayerPopulation },
                    { AccountSettingEnums.InactiveFinderMaxPlayerPopulation, maxPlayerPopulation },
                });

                server = context.Accounts
                    .Where(x => x.Id == AccountId.Value)
                    .Select(x => x.Server)
                    .First();
                var villages = context.Villages
                    .Where(x => x.AccountId == AccountId.Value)
                    .Select(x => new { x.Id, x.X, x.Y })
                    .ToList();
                ownVillages = villages.Select(x => (x.X, x.Y)).ToList();
                var centerVillage = villages.First(x => x.Id == center.Id);
                centerCoordinates = (centerVillage.X, centerVillage.Y);
            }

            var download = await _mapDataService.DownloadToday(server, cancellationToken);

            var search = _mapDataService.FindInactives(server, centerCoordinates.X, centerCoordinates.Y, radius, minPopulation, maxPopulation, minPlayerPopulation, maxPlayerPopulation, InactiveDays, ownVillages);
            if (search.IsFailed)
            {
                var errors = search.Errors.Concat(download.Errors).Select(x => x.Message);
                return new(string.Join(Environment.NewLine, errors), []);
            }

            var (older, newer, found) = search.Value;
            var items = found
                .Select(x => new InactiveVillageItem(
                    x.Distance.ToString("0.0"),
                    $"({x.Village.X}|{x.Village.Y})",
                    x.Village.VillageName,
                    x.Village.PlayerName,
                    x.Village.AllianceTag,
                    x.Village.Population,
                    $"{x.PlayerPopulationBefore} -> {x.PlayerPopulationNow}"))
                .ToList();

            var status = older is null
                ? $"{items.Count} villages within {radius} fields of ({centerCoordinates.X}|{centerCoordinates.Y}), filtered by population only. Not enough map history to tell inactive players yet (needs data from {InactiveDays} days ago, available from {MapDataService.GetSnapshotDates(server)[0].AddDays(InactiveDays):yyyy-MM-dd}). These players may be active."
                : $"{items.Count} inactive villages within {radius} fields of ({centerCoordinates.X}|{centerCoordinates.Y}). Player population did not grow between {older:yyyy-MM-dd} and {newer:yyyy-MM-dd}.";
            return new(status, items);
        }

        [ReactiveCommand(RunInBackground = true)]
        private async Task AddInactivesToFarmList()
        {
            var farmList = FarmLists.SelectedItem;
            if (farmList is null)
            {
                await _dialogService.SendMessage("Warning", "Select the farm list to add the targets to. Use Load farmlist if the list is empty.");
                return;
            }

            var targets = InactiveVillages
                .Select(x => ParseCoordinates(x.Coordinates))
                .ToList();
            if (targets.Count == 0)
            {
                await _dialogService.SendMessage("Warning", "Find inactives first");
                return;
            }

            var validation = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput);
            if (!validation.IsValid)
            {
                await _dialogService.SendMessage("Error", string.Join(Environment.NewLine, validation.Errors.Select(x => x.ErrorMessage)));
                return;
            }

            var troopSlot = AccountSettingInput.InactiveFinderTroopSlot.Get();
            var troopAmount = AccountSettingInput.InactiveFinderTroopAmount.Get();
            using (var context = _contextFactory.CreateDbContext())
            {
                context.SaveAccountSetting(AccountId, new()
                {
                    { AccountSettingEnums.InactiveFinderTroopSlot, troopSlot },
                    { AccountSettingEnums.InactiveFinderTroopAmount, troopAmount },
                });
            }

            var confirmed = await _dialogService.SendConfirm("Add to farm list", $"Add {targets.Count} villages to farm list \"{farmList.Content}\" with {troopAmount} x t{troopSlot} each? Targets already in the list are skipped.");
            if (!confirmed) return;

            _taskManager.Remove<AddFarmTargetsTask.Task>(AccountId);
            _taskManager.Add(new AddFarmTargetsTask.Task(AccountId, new FarmId(farmList.Id), targets, troopSlot, troopAmount), first: true);
            await _dialogService.SendMessage("Information", "Added task to add the targets to the farm list");
        }

        private static (int X, int Y) ParseCoordinates(string coordinates)
        {
            var parts = coordinates.Trim('(', ')').Split('|');
            return (int.Parse(parts[0]), int.Parse(parts[1]));
        }

        [Reactive]
        private string _inactiveStatus = "Map data is downloaded once a day. Inactive players can be found once there is data from 2 days ago.";

        [Reactive]
        private List<InactiveVillageItem> _inactiveVillages = [];

        [Reactive]
        private string _activeText = "No farmlist selected";
    }
}