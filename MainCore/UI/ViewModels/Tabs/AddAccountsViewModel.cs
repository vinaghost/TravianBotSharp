using System.Collections.ObjectModel;
using System.Text.Json;

using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.UI.ViewModels.Tabs
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<AddAccountsViewModel>]
    public partial class AddAccountsViewModel : TabViewModelBase
    {
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly RxQueue _rxQueue;
        private readonly DefaultTemplatePathStore _defaultTemplatePathStore;
        public ObservableCollection<AccountDetailDto> Accounts { get; } = [];

        [Reactive]
        private string _input = "";

        public AddAccountsViewModel(DialogService dialogService, WaitingOverlayViewModel waitingOverlayViewModel, IDbContextFactory<AppDbContext> contextFactory, RxQueue rxQueue, DefaultTemplatePathStore defaultTemplatePathStore) : base(dialogService)
        {
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _contextFactory = contextFactory;
            _rxQueue = rxQueue;
            _defaultTemplatePathStore = defaultTemplatePathStore;

            Init();
        }

        private void Init()
        {
            this.WhenAnyValue(x => x.Input)
                .ObserveOn(RxSchedulers.TaskpoolScheduler)
                .InvokeCommand(ParseCommand);

            ParseCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(UpdateTable);

            AddAccountCommand.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(_ => Clear());
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

            _waitingOverlayViewModel.Show("adding accounts");

            try
            {
                await AddToDatabaseCommand.Execute(dtos);
                _rxQueue.Enqueue(new AccountsModified());
            }
            finally
            {
                _waitingOverlayViewModel.Hide();
            }

            await _dialogService.SendMessage("Information", $"Added {dtos.Count} accounts");
        }

        [ReactiveCommand(RunInBackground = true)]
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

        private List<AccountDetailDto> FilterDuplicated(List<AccountDetailDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var existAccounts = context.Accounts
                .Select(x => new { x.Username, x.Server })
                .AsEnumerable()
                .Select(x => $"{x.Username}-{x.Server}")
                .ToHashSet();

            return [.. dtos.Where(x => !existAccounts.Contains($"{x.Username}-{x.Server}"))];
        }

        [ReactiveCommand(RunInBackground = true)]
        private void AddToDatabase(List<AccountDetailDto> dtos)
        {
            var defaultSettings = LoadAccountDefaultSettings();

            var accounts = dtos
                .Select(dto => new Account()
                {
                    Username = dto.Username,
                    Server = dto.Server,
                    Accesses = [ new Access()
                    {
                        Username = dto.Username,
                        Password = dto.Password,
                        ProxyHost = dto.ProxyHost,
                        ProxyPort = dto.ProxyPort,
                        ProxyUsername = dto.ProxyUsername,
                        ProxyPassword = dto.ProxyPassword,
                    }],
                    Settings = [],
                });

            using var context = _contextFactory.CreateDbContext();
            foreach (var account in accounts)
            {
                foreach (var (setting, value) in defaultSettings)
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

        private Dictionary<AccountSettingEnums, int> LoadAccountDefaultSettings()
        {
            var settings = AppDbContext.AccountDefaultSettings
                .ToDictionary(x => x.Key, x => x.Value);

            var path = _defaultTemplatePathStore.Get().AccountSettingsPath;
            if (string.IsNullOrWhiteSpace(path)) return settings;
            if (!File.Exists(path)) return settings;

            try
            {
                var jsonString = File.ReadAllText(path);
                var importedSettings = JsonSerializer.Deserialize<Dictionary<AccountSettingEnums, int>>(jsonString);
                if (importedSettings is null) return settings;

                foreach (var (setting, value) in importedSettings)
                {
                    if (setting == AccountSettingEnums.Tribe) continue;
                    settings[setting] = value;
                }
            }
            catch
            {
                return settings;
            }

            return settings;
        }
    }
}