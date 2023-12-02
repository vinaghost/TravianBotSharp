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
        private readonly IUnitOfRepository _unitOfRepository;

        private readonly List<BuildingEnums> _availableBuildings = new();

        private ReactiveCommand<ListBoxItem, Unit> BuildingChanged { get; }

        public ReactiveCommand<Unit, Unit> BuildNormal { get; }
        public ReactiveCommand<Unit, Unit> BuildResource { get; }

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

        public NormalBuildInput NormalBuildInput { get; } = new();

        public ResourceBuildInput ResourceBuildInput { get; } = new();

        public ListBoxItemViewModel Buildings { get; } = new();
        public ListBoxItemViewModel Jobs { get; } = new();

        public BuildViewModel(IMediator mediator, IUnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;

            BuildingChanged = ReactiveCommand.CreateFromTask<ListBoxItem>(BuildingChangedHandler);

            BuildNormal = ReactiveCommand.CreateFromTask(BuildNormalHandler);
            BuildResource = ReactiveCommand.CreateFromTask(ResourceNormalHandler);

            Up = ReactiveCommand.CreateFromTask(UpHandler);
            Down = ReactiveCommand.CreateFromTask(DownHandler);
            Top = ReactiveCommand.CreateFromTask(TopHandler);
            Bottom = ReactiveCommand.CreateFromTask(BottomHandler);
            Delete = ReactiveCommand.CreateFromTask(DeleteHandler);
            DeleteAll = ReactiveCommand.CreateFromTask(DeleteAllHandler);
            Import = ReactiveCommand.CreateFromTask(ImportHandler);
            Export = ReactiveCommand.CreateFromTask(ExportHandler);

            LoadBuilding = ReactiveCommand.CreateFromTask<VillageId, List<ListBoxItem>>(LoadBuildingHandler);
            LoadJob = ReactiveCommand.CreateFromTask<VillageId, List<ListBoxItem>>(LoadJobHandler);

            this.WhenAnyValue(vm => vm.Buildings.SelectedItem)
                .InvokeCommand(BuildingChanged);

            for (var i = BuildingEnums.Sawmill; i <= BuildingEnums.Hospital; i++)
            {
                _availableBuildings.Add(i);
            }

            LoadBuilding.Subscribe(buildings => Buildings.Load(buildings));
            LoadJob.Subscribe(jobs => Jobs.Load(jobs));
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
        }

        private async Task<List<ListBoxItem>> LoadBuildingHandler(VillageId villageId)
        {
            var buildings = await Task.Run(() => _unitOfRepository.BuildingRepository.GetItems(villageId));
            return buildings;
        }

        private async Task<List<ListBoxItem>> LoadJobHandler(VillageId villageId)
        {
            var buildings = await Task.Run(() => _unitOfRepository.JobRepository.GetItems(villageId));
            return buildings;
        }

        private async Task BuildingChangedHandler(ListBoxItem item)
        {
            await Task.CompletedTask;
            if (item is null)
            {
                NormalBuildInput.Clear();
            }
            else
            {
                var (type, level) = _unitOfRepository.BuildingRepository.GetBuildingInfo(new BuildingId(Buildings.SelectedItemId));

                if (type == BuildingEnums.Site)
                {
                    NormalBuildInput.Set(_availableBuildings);
                }
                else
                {
                    NormalBuildInput.Set(new List<BuildingEnums>() { type }, level + 1);
                }
            }
        }

        private async Task BuildNormalHandler()
        {
            var location = Buildings.SelectedIndex + 1;
            await _mediator.Send(new BuildNormalCommand(AccountId, VillageId, NormalBuildInput, location));
        }

        private async Task ResourceNormalHandler()
        {
            await _mediator.Send(new BuildResourceCommand(AccountId, VillageId, ResourceBuildInput));
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