using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using System.Collections.ObjectModel;

namespace MainCore.UI.ViewModels.Tabs
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<AddAccountsViewModel>]
    public partial class AddAccountsViewModel : TabViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IRxQueue _rxQueue;
        public ObservableCollection<AccountDetailDto> Accounts { get; } = [];

        [Reactive]
        private string _input = "";

        public AddAccountsViewModel(IDialogService dialogService, IWaitingOverlayViewModel waitingOverlayViewModel, IDbContextFactory<AppDbContext> contextFactory, IRxQueue rxQueue)
        {
            _dialogService = dialogService;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _contextFactory = contextFactory;
            _rxQueue = rxQueue;

            Init();
        }

        private void Init()
        {
            this.WhenAnyValue(x => x.Input)
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
                .InvokeCommand(ParseCommand);

            ParseCommand.Subscribe(UpdateTable);

            AddAccountCommand.Subscribe(_ => Clear());
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

        [ReactiveCommand]
        private async Task AddAccount()
        {
            var dtos = FilterDuplicated([.. Accounts]);
            if (dtos.Count == 0)
            {
                await _dialogService.SendMessage("Information", "All accounts are duplicated");
                return;
            }

            await _waitingOverlayViewModel.Show("adding accounts");
            await Signal.Start(() => UpdateDatabase(dtos), RxSchedulers.TaskpoolScheduler);

            _rxQueue.Enqueue(new AccountsModified());
            await _waitingOverlayViewModel.Hide();

            await _dialogService.SendMessage("Information", $"Added accounts");
        }

        [ReactiveCommand]
        private static List<AccountDetailDto> Parse(string input)
        {
            if (string.IsNullOrEmpty(input)) return [];

            var accounts = input
                .Trim()
                .Split('\n')
                .AsParallel()
                .Select(ParseLine)
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();

            return accounts;
        }

        private static AccountDetailDto? ParseLine(string input)
        {
            var strAccount = input.Trim().Split(' ');
            if (strAccount.Length < 3 || strAccount.Length > 7)
                return null;

            if (string.IsNullOrWhiteSpace(strAccount[0]))
                return null;

            if (!Uri.TryCreate(strAccount[0], UriKind.Absolute, out var uri))
                return null;

            if (uri is null)
                return null;

            if (strAccount.Length > 4)
            {
                if (!int.TryParse(strAccount[4], out var port))
                    return null;

                strAccount[4] = port.ToString();
            }

            var serverUrl = $"{uri.Scheme}://{uri.Host}";

            return strAccount.Length switch
            {
                3 => AccountDetailDto.Create(strAccount[1], serverUrl, strAccount[2]),
                5 => AccountDetailDto.Create(strAccount[1], serverUrl, strAccount[2], strAccount[3], int.Parse(strAccount[4])),
                7 => AccountDetailDto.Create(strAccount[1], serverUrl, strAccount[2], strAccount[3], int.Parse(strAccount[4]), strAccount[5], strAccount[6]),
                _ => null,
            };
        }

        private List<AccountDto> FilterDuplicated(List<AccountDetailDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var existAccounts = context.Accounts
                .Select(x => new
                {
                    x.Username,
                    x.Server,
                })
                .ToList();

            return [.. dtos
                .Select(x => x.ToDto())
                .Where(dto => !existAccounts.Exists(x => x.Username == dto.Username && x.Server == dto.Server))];
        }

        private void UpdateDatabase(List<AccountDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();

            var accounts = dtos
                .Select(x => x.ToEntity());

            foreach (var account in accounts)
            {
                account.Settings = [];
                foreach (var (setting, value) in AppDbContext.AccountDefaultSettings)
                {
                    account.Settings.Add(new AccountSetting
                    {
                        Setting = setting,
                        Value = value,
                    });
                }
                context.Add(account);
            }

            context.SaveChanges();
        }
    }
}