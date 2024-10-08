using MainCore.Commands.UI.Tabs;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<AddAccountsViewModel>]
    public class AddAccountsViewModel : TabViewModelBase
    {
        public ReactiveCommand<Unit, Unit> AddAccount { get; }
        private ReactiveCommand<string, List<AccountDetailDto>> Parse { get; }

        public ObservableCollection<AccountDetailDto> Accounts { get; } = [];
        private string _input;

        public string Input
        {
            get => _input;
            set => this.RaiseAndSetIfChanged(ref _input, value);
        }

        public AddAccountsViewModel()
        {
            AddAccount = ReactiveCommand.CreateFromTask(AddAccountHandler);
            Parse = ReactiveCommand.Create<string, List<AccountDetailDto>>(ParseHandler);

            this.WhenAnyValue(x => x.Input)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InvokeCommand(Parse);

            Parse.Subscribe(UpdateTable);

            AddAccount.Subscribe(_ => Clear());
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
            var addAccountCommand = Locator.Current.GetService<AddAccountCommand>();
            await addAccountCommand.Execute([.. Accounts], default);
        }

        private static List<AccountDetailDto> ParseHandler(string input)
        {
            if (string.IsNullOrEmpty(input)) return [];

            var accounts = input
                .Trim()
                .Split('\n')
                .AsParallel()
                .Select(ParseLine)
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
    }
}