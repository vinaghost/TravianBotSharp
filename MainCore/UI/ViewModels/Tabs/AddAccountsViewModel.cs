using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsViewModel]
    public class AddAccountsViewModel : TabViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IUseragentManager _useragentManager;

        public ReactiveCommand<Unit, Unit> AddAccount { get; }
        private ReactiveCommand<string, List<AccountDetailDto>> Parse { get; }

        public ObservableCollection<AccountDetailDto> Accounts { get; } = new();
        private string _input;

        public string Input
        {
            get => _input;
            set => this.RaiseAndSetIfChanged(ref _input, value);
        }

        public AddAccountsViewModel(IMediator mediator, IDialogService dialogService, WaitingOverlayViewModel waitingOverlayViewModel, IDbContextFactory<AppDbContext> contextFactory, IUseragentManager useragentManager)
        {
            _mediator = mediator;
            _dialogService = dialogService;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _contextFactory = contextFactory;
            _useragentManager = useragentManager;

            AddAccount = ReactiveCommand.CreateFromTask(AddAccountHandler);
            Parse = ReactiveCommand.Create<string, List<AccountDetailDto>>(ParseHandler);

            this.WhenAnyValue(x => x.Input)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(Parse);

            Parse.Subscribe(UpdateTable);

            AddAccount.Subscribe(x => Clear());
        }

        private void UpdateTable(List<AccountDetailDto> data)
        {
            Accounts.Clear();
            data.ForEach(Accounts.Add);
        }

        private void Clear()
        {
            Accounts.Clear();
            Input = "";
        }

        private async Task AddAccountHandler()
        {
            await _waitingOverlayViewModel.Show("adding accounts");

            Add(Accounts.ToList());

            await _mediator.Publish(new AccountUpdated());

            await _waitingOverlayViewModel.Hide();

            _dialogService.ShowMessageBox("Information", "Added accounts");
        }

        private List<AccountDetailDto> ParseHandler(string input)
        {
            if (string.IsNullOrEmpty(input)) return new List<AccountDetailDto>();

            var accounts = input
                .Trim()
                .Split('\n')
                .AsParallel()
                .Select(x => ParseLine(x))
                .Where(x => x is not null)
                .ToList();

            return accounts;
        }

        private static AccountDetailDto ParseLine(string input)
        {
            var strAccount = input.Trim().Split(' ');
            Uri url = null;
            if (strAccount.Length > 0 && !Uri.TryCreate(strAccount[0], UriKind.Absolute, out url))
            {
                return null;
            }

            if (url is null) return null;

            if (strAccount.Length > 4)
            {
                if (int.TryParse(strAccount[4], out var port))
                {
                    strAccount[4] = port.ToString();
                }
                else
                {
                    return null;
                }
            }

            var host = url.GetLeftPart(UriPartial.Authority);

            return strAccount.Length switch
            {
                3 => AccountDetailDto.Create(strAccount[1], host, strAccount[2]),
                5 => AccountDetailDto.Create(strAccount[1], host, strAccount[2], strAccount[3], int.Parse(strAccount[4])),
                7 => AccountDetailDto.Create(strAccount[1], host, strAccount[2], strAccount[3], int.Parse(strAccount[4]), strAccount[5], strAccount[6]),
                _ => null,
            };
        }

        private void Add(List<AccountDetailDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();

            var accounts = new List<Account>();
            foreach (var dto in dtos)
            {
                var isExist = context.Accounts
                    .Where(x => x.Username == dto.Username)
                    .Where(x => x.Server == dto.Server)
                    .Any();
                if (isExist) continue;
                var account = dto.ToEnitty();
                foreach (var access in account.Accesses.Where(access => string.IsNullOrEmpty(access.Useragent)))
                {
                    access.Useragent = _useragentManager.Get();
                }

                accounts.Add(account);
            }
            context.AddRange(accounts);
            context.SaveChanges();

            foreach (var account in accounts)
            {
                context.FillAccountSettings(new(account.Id));
            }
        }
    }
}