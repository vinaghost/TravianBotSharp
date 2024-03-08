using DynamicData;
using Humanizer;
using MainCore.Commands.UI.Hero;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MediatR;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class HeroViewModel : AccountTabViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly UnitOfRepository _unitOfRepository;
        public ObservableCollection<AdventureItem> Adventures { get; } = new();
        public ObservableCollection<HeroItemItem> Inventory { get; } = new();
        public ObservableCollection<HeroItemItem> Equipt { get; } = new();
        public ReactiveCommand<Unit, Unit> AdventuresCommand { get; }
        public ReactiveCommand<Unit, Unit> InventoryCommand { get; }

        public ReactiveCommand<AccountId, List<AdventureItem>> LoadAdventuresCommand { get; }
        public ReactiveCommand<AccountId, List<HeroItemItem>> LoadInventoryCommand { get; }

        public ReactiveCommand<AccountId, HeroDto> LoadHeroCommand { get; }

        public HeroViewModel(UnitOfRepository unitOfRepository, IMediator mediator)
        {
            _unitOfRepository = unitOfRepository;

            AdventuresCommand = ReactiveCommand.CreateFromTask(AdventuresCommandHandler);
            InventoryCommand = ReactiveCommand.CreateFromTask(InventoryCommandHandler);

            LoadHeroCommand = ReactiveCommand.Create<AccountId, HeroDto>(LoadHeroHandler);
            LoadAdventuresCommand = ReactiveCommand.Create<AccountId, List<AdventureItem>>(LoadAdventureHandler);
            LoadInventoryCommand = ReactiveCommand.Create<AccountId, List<HeroItemItem>>(LoadInventoryHandler);

            LoadHeroCommand
                .WhereNotNull()
                .Subscribe(x =>
                {
                    Health = x.Health;
                    Status = x.Status.Humanize();
                });

            LoadAdventuresCommand
                .Subscribe(x =>
                {
                    Adventures.Clear();
                    Adventures.AddRange(x);

                    AdventureNum = x.Count;
                });

            LoadInventoryCommand
                .Subscribe(x =>
                {
                    Inventory.Clear();
                    Inventory.AddRange(x);
                });
            _mediator = mediator;
        }

        public async Task HeroRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadHeroCommand.Execute(accountId);
        }

        public async Task AdventureRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadAdventuresCommand.Execute(accountId);
        }

        public async Task InventoryRefresh(AccountId accountId)
        {
            if (!IsActive) return;
            if (accountId != AccountId) return;
            await LoadInventoryCommand.Execute(accountId);
        }

        private async Task AdventuresCommandHandler()
        {
            await _mediator.Send(new UpdateAdventureCommand(AccountId));
        }

        private async Task InventoryCommandHandler()
        {
            await _mediator.Send(new UpdateInventoryCommand(AccountId));
        }

        private HeroDto LoadHeroHandler(AccountId accountId)
        {
            var dto = _unitOfRepository.HeroRepository.Get(accountId);
            return dto;
        }

        private List<AdventureItem> LoadAdventureHandler(AccountId accountId)
        {
            var dtos = _unitOfRepository.AdventureRepository.Get(accountId);
            return dtos.Select(x => new AdventureItem(x)).ToList();
        }

        private List<HeroItemItem> LoadInventoryHandler(AccountId accountId)
        {
            var dtos = _unitOfRepository.HeroItemRepository.GetItems(accountId);
            return dtos.Select(x => new HeroItemItem(x)).ToList();
        }

        private int _health;

        public int Health
        {
            get => _health;
            set => this.RaiseAndSetIfChanged(ref _health, value);
        }

        private string _status;

        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        private int _adventureNum;

        public int AdventureNum
        {
            get => _adventureNum;
            set => this.RaiseAndSetIfChanged(ref _adventureNum, value);
        }

        protected override async Task Load(AccountId accountId)
        {
            await LoadHeroCommand.Execute(accountId);
            await LoadInventoryCommand.Execute(accountId);
            await LoadAdventuresCommand.Execute(accountId);
        }
    }
}