using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class InfoViewModel : VillageTabViewModelBase
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly ITaskManager _taskManager;

        public InfoViewModel(UnitOfRepository unitOfRepository, ITaskManager taskManager)
        {
            _unitOfRepository = unitOfRepository;
            _taskManager = taskManager;

            LoadAmountSettler = ReactiveCommand.Create<VillageId, int>(LoadAmountSettlerHandler);
            LoadExpansionSlot = ReactiveCommand.Create<VillageId, string>(LoadExpansionSlotHandler);

            Test = ReactiveCommand.CreateFromTask(TestHandler);

            LoadAmountSettler.Subscribe(x => SettleAmount = x);
            LoadExpansionSlot.Subscribe(x => ExpansionSlot = x);
        }

        public ReactiveCommand<VillageId, int> LoadAmountSettler;
        public ReactiveCommand<VillageId, string> LoadExpansionSlot;

        public ReactiveCommand<Unit, Unit> Test;

        private int _amountSettler;

        public int SettleAmount
        {
            get => _amountSettler;
            set => this.RaiseAndSetIfChanged(ref _amountSettler, value);
        }

        private string _expansionSlot;

        public string ExpansionSlot
        {
            get => _expansionSlot;
            set => this.RaiseAndSetIfChanged(ref _expansionSlot, value);
        }

        protected override async Task Load(VillageId villageId)
        {
            await LoadAmountSettler.Execute(villageId);
            await LoadExpansionSlot.Execute(villageId);
        }

        private int LoadAmountSettlerHandler(VillageId villageId)
        {
            return _unitOfRepository.VillageRepository.GetSettlers(villageId);
        }

        private string LoadExpansionSlotHandler(VillageId villageId)
        {
            return _unitOfRepository.ExpansionSlotRepository.GetExpansionSlot(villageId);
        }

        private async Task TestHandler()
        {
            await _taskManager.Add<EvadeTroopTask>(AccountId, VillageId, first: true);
        }
    }
}