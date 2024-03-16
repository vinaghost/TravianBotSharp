using MainCore.Commands.UI.Build;
using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using MediatR;
using ReactiveUI;
using System.Reactive.Linq;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class BuildViewModel : VillageTabViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly UnitOfRepository _unitOfRepository;

        private ReactiveCommand<ListBoxItem, List<BuildingEnums>> LoadBuildNormal { get; }

        public ReactiveCommand<Unit, Unit> BuildNormal { get; }
        public ReactiveCommand<Unit, Unit> BuildResource { get; }
        public ReactiveCommand<Unit, Unit> UpgradeOneLevel { get; }
        public ReactiveCommand<Unit, Unit> UpgradeMaxLevel { get; }
        public ReactiveCommand<Unit, Unit> TrainTroop { get; }
        public ReactiveCommand<Unit, Unit> ResearchTroop { get; }
        public ReactiveCommand<Unit, Unit> Celebration { get; }

        public ReactiveCommand<Unit, Unit> Up { get; }
        public ReactiveCommand<Unit, Unit> Down { get; }
        public ReactiveCommand<Unit, Unit> Top { get; }
        public ReactiveCommand<Unit, Unit> Bottom { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }
        public ReactiveCommand<Unit, Unit> DeleteAll { get; }
        public ReactiveCommand<Unit, Unit> Import { get; }
        public ReactiveCommand<Unit, Unit> Export { get; }
        public ReactiveCommand<VillageId, List<ListBoxItem>> LoadBuilding { get; }
        public ReactiveCommand<VillageId, List<ListBoxItem>> LoadJob { get; }
        public ReactiveCommand<VillageId, List<ListBoxItem>> LoadQueue { get; }

        public NormalBuildInput NormalBuildInput { get; } = new();

        public ResourceBuildInput ResourceBuildInput { get; } = new();

        public TrainTroopInput TrainTroopInput { get; } = new();
        public ResearchTroopInput ResearchTroopInput { get; } = new();
        public CelebrationInput CelebrationInput { get; } = new();

        public ListBoxItemViewModel Buildings { get; } = new();
        public ListBoxItemViewModel Queue { get; } = new();
        public ListBoxItemViewModel Jobs { get; } = new();

        public BuildViewModel(IMediator mediator, UnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;

            BuildNormal = ReactiveCommand.CreateFromTask(BuildNormalHandler);
            BuildResource = ReactiveCommand.CreateFromTask(ResourceNormalHandler);
            UpgradeOneLevel = ReactiveCommand.CreateFromTask(UpgradeOneLevelHandler);
            UpgradeMaxLevel = ReactiveCommand.CreateFromTask(UpgradeMaxLevelHandler);

            TrainTroop = ReactiveCommand.CreateFromTask(TrainTroopHandler);
            ResearchTroop = ReactiveCommand.CreateFromTask(ResearchTroopHandler);
            Celebration = ReactiveCommand.CreateFromTask(CelebrationHandler);

            Up = ReactiveCommand.CreateFromTask(UpHandler);
            Down = ReactiveCommand.CreateFromTask(DownHandler);
            Top = ReactiveCommand.CreateFromTask(TopHandler);
            Bottom = ReactiveCommand.CreateFromTask(BottomHandler);
            Delete = ReactiveCommand.CreateFromTask(DeleteHandler);
            DeleteAll = ReactiveCommand.CreateFromTask(DeleteAllHandler);
            Import = ReactiveCommand.CreateFromTask(ImportHandler);
            Export = ReactiveCommand.CreateFromTask(ExportHandler);

            LoadBuilding = ReactiveCommand.Create<VillageId, List<ListBoxItem>>(LoadBuildingHandler);
            LoadJob = ReactiveCommand.Create<VillageId, List<ListBoxItem>>(LoadJobHandler);
            LoadQueue = ReactiveCommand.Create<VillageId, List<ListBoxItem>>(LoadQueueHandler);
            LoadBuildNormal = ReactiveCommand.Create<ListBoxItem, List<BuildingEnums>>(LoadBuildNormalHanlder);

            this.WhenAnyValue(vm => vm.Buildings.SelectedItem)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(LoadBuildNormal);

            LoadBuilding.Subscribe(buildings => Buildings.Load(buildings));
            LoadJob.Subscribe(jobs => Jobs.Load(jobs));
            LoadQueue.Subscribe(queue => Queue.Load(queue));
            LoadBuildNormal.Subscribe(buildings =>
            {
                switch (buildings.Count)
                {
                    case 0:
                        NormalBuildInput.Clear();
                        break;

                    default:
                        NormalBuildInput.Set(buildings, -1);
                        break;
                };
            });
        }

        public async Task QueueRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadQueue.Execute(villageId);
        }

        public async Task BuildingListRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadBuilding.Execute(villageId);
        }

        public async Task JobListRefresh(VillageId villageId)
        {
            if (!IsActive) return;
            if (villageId != VillageId) return;
            await LoadJob.Execute(villageId);
            await LoadBuilding.Execute(villageId);
        }

        protected override async Task Load(VillageId villageId)
        {
            await LoadJob.Execute(villageId);
            await LoadBuilding.Execute(villageId);
            await LoadQueue.Execute(villageId);

            var tribe = (TribeEnums)_unitOfRepository.VillageSettingRepository.GetByName(villageId, VillageSettingEnums.Tribe);

            TrainTroopInput.Set(tribe);
            ResearchTroopInput.Set(tribe);
        }

        private List<ListBoxItem> LoadBuildingHandler(VillageId villageId)
        {
            var buildings = _unitOfRepository.BuildingRepository.GetItems(villageId);
            return buildings;
        }

        private List<ListBoxItem> LoadQueueHandler(VillageId villageId)
        {
            var queue = _unitOfRepository.QueueBuildingRepository.GetItems(villageId);
            return queue;
        }

        private List<ListBoxItem> LoadJobHandler(VillageId villageId)
        {
            var jobs = _unitOfRepository.JobRepository.GetItems(villageId);
            return jobs;
        }

        private List<BuildingEnums> LoadBuildNormalHanlder(ListBoxItem item)
        {
            if (item is null) return new();
            var buildings = _unitOfRepository.BuildingRepository.GetNormalBuilding(VillageId, new BuildingId(item.Id));
            return buildings;
        }

        private async Task BuildNormalHandler()
        {
            var location = Buildings.SelectedIndex + 1;
            await _mediator.Send(new BuildNormalCommand(AccountId, VillageId, NormalBuildInput, location));
        }

        private async Task UpgradeOneLevelHandler()
        {
            var location = Buildings.SelectedIndex + 1;
            await _mediator.Send(new UpgradeLevelCommand(AccountId, VillageId, location, false));
        }

        private async Task UpgradeMaxLevelHandler()
        {
            var location = Buildings.SelectedIndex + 1;
            await _mediator.Send(new UpgradeLevelCommand(AccountId, VillageId, location, true));
        }

        private async Task ResourceNormalHandler()
        {
            await _mediator.Send(new BuildResourceCommand(AccountId, VillageId, ResourceBuildInput));
        }

        private async Task TrainTroopHandler()
        {
            await _mediator.Send(new TrainTroopCommand(AccountId, VillageId, TrainTroopInput));
        }

        private async Task ResearchTroopHandler()
        {
            await _mediator.Send(new ResearchTroopCommand(AccountId, VillageId, ResearchTroopInput));
        }

        private async Task CelebrationHandler()
        {
            await _mediator.Send(new CelebrationCommand(AccountId, VillageId, CelebrationInput));
        }

        private async Task UpHandler()
        {
            await _mediator.Send(new MoveJobCommand(AccountId, VillageId, Jobs, MoveEnums.Up));
        }

        private async Task DownHandler()
        {
            await _mediator.Send(new MoveJobCommand(AccountId, VillageId, Jobs, MoveEnums.Down));
        }

        private async Task TopHandler()
        {
            await _mediator.Send(new MoveJobCommand(AccountId, VillageId, Jobs, MoveEnums.Top));
        }

        private async Task BottomHandler()
        {
            await _mediator.Send(new MoveJobCommand(AccountId, VillageId, Jobs, MoveEnums.Bottom));
        }

        private async Task DeleteHandler()
        {
            await _mediator.Send(new DeleteJobCommand(AccountId, VillageId, Jobs));
        }

        private async Task DeleteAllHandler()
        {
            await _mediator.Send(new DeleteAllJobCommand(AccountId, VillageId, Jobs));
        }

        private async Task ImportHandler()
        {
            await _mediator.Send(new ImportCommand(AccountId, VillageId));
        }

        private async Task ExportHandler()
        {
            await _mediator.Send(new ExportCommand(VillageId));
        }
    }
}