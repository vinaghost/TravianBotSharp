using DynamicData;
using MainCore.Commands.UI.AccountSettingViewModel;
using MainCore.Commands.UI.Misc;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<HeroFarmingViewModel>]
    public partial class HeroFarmingViewModel : AccountTabViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;
        private readonly IValidator<AccountSettingInput> _accountsettingInputValidator;

        public AccountSettingInput AccountSettingInput { get; } = new();

        [Reactive]
        private int _x;

        [Reactive]
        private int _y;

        public HeroFarmGridViewModel Oasises { get; } = new();

        public HeroFarmingViewModel(IDialogService dialogService, ICustomServiceScopeFactory serviceScopeFactory, ITaskManager taskManager, IValidator<AccountSettingInput> accountsettingInputValidator)
        {
            _dialogService = dialogService;
            _serviceScopeFactory = serviceScopeFactory;
            _taskManager = taskManager;
            _accountsettingInputValidator = accountsettingInputValidator;

            LoadOasisCommand.Subscribe(Oasises.Load);
            LoadSettingCommand.Subscribe(AccountSettingInput.Set);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadOasisCommand.Execute(accountId);
            await LoadSettingCommand.Execute(accountId);
        }

        [ReactiveCommand]
        private async Task<List<HeroFarmItem>> LoadOasis(AccountId accountId)
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getHeroFarmTargetQuery = scope.ServiceProvider.GetRequiredService<GetHeroFarmTargetQuery.Handler>();
            var items = await getHeroFarmTargetQuery.HandleAsync(new(accountId));
            return items;
        }

        [ReactiveCommand]
        private async Task<Dictionary<AccountSettingEnums, int>> LoadSetting(AccountId accountId)
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var getSettingQuery = scope.ServiceProvider.GetRequiredService<GetSettingQuery.Handler>();
            var items = await getSettingQuery.HandleAsync(new(accountId));
            return items;
        }

        [ReactiveCommand]
        private async Task Start()
        {
            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            _taskManager.AddOrUpdate<StartHeroFarmingTask.Task>(new(AccountId));
            await _dialogService.MessageBox.Handle(input: new MessageBoxData("Information", "Added start hero farming task"));
        }

        [ReactiveCommand]
        private async Task Stop()
        {
            _taskManager.Remove<StartHeroFarmingTask.Task>(AccountId);
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Removed start hero farming task"));
        }

        [ReactiveCommand]
        private async Task Save()
        {
            var result = await _accountsettingInputValidator.ValidateAsync(AccountSettingInput);
            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var saveAccountSettingCommand = scope.ServiceProvider.GetRequiredService<SaveAccountSettingCommand.Handler>();
            await saveAccountSettingCommand.HandleAsync(new(AccountId, AccountSettingInput.Get()));
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Saved"));
        }

        [ReactiveCommand]
        private async Task Add()
        {
            if (Oasises.Items.Any(x => x.X == X && x.Y == Y))
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Target already exists"));
                return;
            }
            var item = new HeroFarmItem
            {
                Id = Oasises.Items.Count + 1,
                X = X,
                Y = Y,
                LastSend = DateTime.MinValue,
            };

            Oasises.Items.Add(item);
            Oasises.SelectedItem = item;

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.HeroFarmTargets.Add(new HeroFarmTarget
            {
                AccountId = AccountId.Value,
                X = item.X,
                Y = item.Y,
                Animal = item.Animal,
                Resource = item.Resource,
                OasisType = item.OasisType,
                LastSend = item.LastSend
            });
            context.SaveChanges();
        }

        [ReactiveCommand]
        private async Task Up()
        {
            if (Oasises.SelectedItem is null)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }
            var selectedItem = Oasises.SelectedItem;
            var index = Oasises.Items.IndexOf(selectedItem);
            if (index <= 0) return;
            Oasises.Items.Move(index, index - 1);
            Oasises.SelectedIndex = index - 1;
        }

        [ReactiveCommand]
        private async Task Down()
        {
            if (Oasises.SelectedItem is null)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }

            var selectedItem = Oasises.SelectedItem;
            var index = Oasises.Items.IndexOf(selectedItem);
            if (index < 0 || index > Oasises.Items.Count) return;
            Oasises.Items.Move(index, index + 1);
            Oasises.SelectedIndex = index + 1;
        }

        [ReactiveCommand]
        private async Task Top()
        {
            if (Oasises.SelectedItem is null)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }
            var selectedItem = Oasises.SelectedItem;
            var index = Oasises.Items.IndexOf(selectedItem);
            if (index <= 0) return;
            Oasises.Items.Move(index, 0);
            Oasises.SelectedIndex = 0;
        }

        [ReactiveCommand]
        private async Task Bottom()
        {
            if (Oasises.SelectedItem is null)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Warning", "Please select before moving"));
                return;
            }

            var selectedItem = Oasises.SelectedItem;
            var index = Oasises.Items.IndexOf(selectedItem);
            if (index < 0 || index > Oasises.Items.Count) return;
            Oasises.Items.Move(index, Oasises.Items.Count - 1);
            Oasises.SelectedIndex = Oasises.Items.Count - 1;
        }

        [ReactiveCommand]
        private async Task Delete()
        {
            if (Oasises.SelectedItem is null) return;
            var selectedItem = Oasises.SelectedItem;
            var confirm = await _dialogService.ConfirmBox.Handle(new MessageBoxData("Warning", $"Are you sure you want to delete {selectedItem.OasisType} ({selectedItem.X}|{selectedItem.Y})?"));
            if (!confirm) return;
            Oasises.Items.Remove(selectedItem);

            using var scope = _serviceScopeFactory.CreateScope(AccountId);

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.HeroFarmTargets
               .Where(x => x.Id == selectedItem.Id)
               .ExecuteDelete();

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Target deleted successfully"));
        }

        [ReactiveCommand]
        private async Task DeleteAll()
        {
            var confirm = await _dialogService.ConfirmBox.Handle(new MessageBoxData("Warning", "Are you sure you want to delete all targets?"));
            if (!confirm) return;
            Oasises.Items.Clear();
            Oasises.SelectedIndex = -1;

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.HeroFarmTargets
                .Where(x => x.AccountId == AccountId.Value)
                .ExecuteDelete();

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "All targets deleted successfully"));
        }

        [ReactiveCommand]
        private async Task Import()
        {
            var path = await _dialogService.OpenFileDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path)) return;

            var data = await File.ReadAllTextAsync(path);
            var oasises = JsonSerializer.Deserialize<List<OasisDto>>(data);
            if (oasises is null || oasises.Count == 0)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", "No valid data found in the file"));
                return;
            }
            var items = oasises.Select(x => new HeroFarmItem
            {
                Id = Oasises.Items.Count + 1,
                X = x.X,
                Y = x.Y,
                LastSend = DateTime.Now,
            }).ToList();

            Oasises.Items.AddRange(items);

            var enities = items.Select(x => new HeroFarmTarget
            {
                AccountId = AccountId.Value,
                X = x.X,
                Y = x.Y,
                Animal = x.Animal,
                Resource = x.Resource,
                OasisType = x.OasisType,
                LastSend = x.LastSend
            });

            using var scope = _serviceScopeFactory.CreateScope(AccountId);
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.HeroFarmTargets.AddRange(enities);
            context.SaveChanges();

            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"{items.Count} targets imported"));
        }

        [ReactiveCommand]
        private async Task Export()
        {
            var path = await _dialogService.SaveFileDialog.Handle(Unit.Default);
            if (string.IsNullOrEmpty(path)) return;

            var oasises = Oasises.Items
                .Select(x => new OasisDto(x.X, x.Y))
                .ToList();

            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(oasises));
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", "Job list exported"));
        }

        public record OasisDto(int X, int Y);
    }
}