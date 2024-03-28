using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class SettleViewModel : AccountTabViewModelBase
    {
        private readonly UnitOfRepository _unitOfrepository;
        private readonly IDialogService _dialogService;

        public ReactiveCommand<Unit, List<ListBoxItem>> LoadNewVillages { get; }
        public ReactiveCommand<Unit, string> LoadPath { get; }
        public ReactiveCommand<Unit, Unit> Add { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }
        public ReactiveCommand<Unit, Unit> Import { get; }

        public SettleViewModel(UnitOfRepository unitOfrepository, IDialogService dialogService)
        {
            _unitOfrepository = unitOfrepository;
            _dialogService = dialogService;

            LoadNewVillages = ReactiveCommand.Create(LoadNewVillagesHandler);
            LoadPath = ReactiveCommand.Create(LoadPathHandler);
            Add = ReactiveCommand.CreateFromTask(AddHandler);
            Delete = ReactiveCommand.CreateFromTask(DeleteHandler);
            Import = ReactiveCommand.CreateFromTask(ImportHandler);

            LoadNewVillages.Subscribe(x =>
            {
                NewVillages.Load(x);
            });
            LoadPath.Subscribe(x =>
            {
                Path = x;
            });
        }

        public ListBoxItemViewModel NewVillages { get; } = new();
        private int _x;

        public int X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        private int _y;

        public int Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        private string _path;

        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        private List<ListBoxItem> LoadNewVillagesHandler()
        {
            var items = _unitOfrepository.NewVillageRepository.GetAll(AccountId);
            string Content(NewVillage village)
            {
                var villageName = _unitOfrepository.VillageRepository.GetVillageName(new VillageId(village.VillageId));
                if (string.IsNullOrEmpty(villageName)) villageName = "available";
                return $"{village.X} | {village.Y} [{villageName}]";
            }
            return items
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Content = Content(x),
                })
                .ToList();
        }

        private string LoadPathHandler()
        {
            return _unitOfrepository.AccountInfoRepository.GetTemplatePath(AccountId);
        }

        private async Task AddHandler()
        {
            if (_unitOfrepository.NewVillageRepository.IsExist(AccountId, X, Y))
            {
                var result = _dialogService.ShowConfirmBox("Warning", "Duplicate coordinate, do you want to reset village");
                if (!result) return;
                _unitOfrepository.NewVillageRepository.Reset(AccountId, X, Y);
            }
            else
            {
                _unitOfrepository.NewVillageRepository.Add(AccountId, X, Y);
            }
            await LoadNewVillages.Execute();
        }

        private async Task DeleteHandler()
        {
            _unitOfrepository.NewVillageRepository.Delete(NewVillages.SelectedItemId);
            await LoadNewVillages.Execute();
        }

        private async Task ImportHandler()
        {
            var path = _dialogService.OpenFileDialog();
            List<JobDto> jobs;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                jobs = JsonSerializer.Deserialize<List<JobDto>>(jsonString);
            }
            catch
            {
                _dialogService.ShowMessageBox("Warning", "Invalid file.");
                return;
            }

            var confirm = _dialogService.ShowConfirmBox("Warning", "TBS will remove resource field build job if its position doesn't match with current village.");
            if (!confirm) return;

            _unitOfrepository.AccountInfoRepository.SetTemplatePath(AccountId, path);
            await LoadPath.Execute();
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadNewVillages.Execute();
            await LoadPath.Execute();
        }
    }
}