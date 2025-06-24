using DynamicData;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<HeroFarmingViewModel>]
    public partial class HeroFarmingViewModel : AccountTabViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ITaskManager _taskManager;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;

        [Reactive]
        private int _x;

        [Reactive]
        private int _y;

        public HeroFarmGridViewModel Oasises { get; } = new();

        public HeroFarmingViewModel(IDialogService dialogService, ICustomServiceScopeFactory serviceScopeFactory, ITaskManager taskManager)
        {
            _dialogService = dialogService;
            _serviceScopeFactory = serviceScopeFactory;
            _taskManager = taskManager;

            LoadOasisCommand.Subscribe(Oasises.Load);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadOasisCommand.Execute(accountId);
        }

        [ReactiveCommand]
        private async Task<List<HeroFarmItem>> LoadOasis(AccountId accountId)
        {
            await Task.CompletedTask;
            var items = new List<HeroFarmItem>()
            {
                new HeroFarmItem { Id= 1, X = 1, Y = 2, LastSend = DateTime.Now.AddSeconds(1), OasisType = "Clay" },
                new HeroFarmItem { Id= 2, X = 3, Y = 4, LastSend = DateTime.Now.AddSeconds(2), OasisType = "Iron" },
                new HeroFarmItem { Id= 3, X = 5, Y = 6, LastSend = DateTime.Now.AddSeconds(3), OasisType = "Crop" },
                new HeroFarmItem { Id= 4, X = 7, Y = 8, LastSend = DateTime.Now.AddSeconds(4), OasisType = "Mixed" },
                new HeroFarmItem { Id= 5, X = 9, Y = 10, LastSend = DateTime.Now.AddSeconds(5), OasisType = "Clay" },
                new HeroFarmItem { Id= 6, X = 11, Y = 12, LastSend = DateTime.Now.AddSeconds(6), OasisType = "Iron" }
            };
            return items;
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
                LastSend = DateTime.Now,
            };
            Oasises.Items.Add(item);
            Oasises.SelectedItem = item;
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
        }

        [ReactiveCommand]
        private async Task DeleteAll()
        {
            var confirm = await _dialogService.ConfirmBox.Handle(new MessageBoxData("Warning", "Are you sure you want to delete all targets?"));
            if (!confirm) return;
            Oasises.Items.Clear();
            Oasises.SelectedIndex = -1;
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