using MainCore.Commands.UI.AddAccounts;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.UI.ViewModels.Abstract;
using MediatR;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class AddAccountsViewModel : TabViewModelBase
    {
        private readonly IMediator _mediator;
        public ReactiveCommand<Unit, Unit> AddAccount { get; }
        private ReactiveCommand<string, List<AccountDetailDto>> Parse { get; }

        public ObservableCollection<AccountDetailDto> Accounts { get; } = new();
        private string _input;

        public string Input
        {
            get => _input;
            set => this.RaiseAndSetIfChanged(ref _input, value);
        }

        public AddAccountsViewModel(IMediator mediator)
        {
            _mediator = mediator;

            AddAccount = ReactiveCommand.CreateFromTask(AddAccountHandler);
            Parse = ReactiveCommand.Create<string, List<AccountDetailDto>>(ParseHandler);

            this.WhenAnyValue(x => x.Input)
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
            await _mediator.Send(new AddAccountsCommand(Accounts.ToList()));
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
            if (strAccount.Length > 0)
            {
                if (!Uri.TryCreate(strAccount[0], UriKind.Absolute, out url))
                {
                    return null;
                };
            }

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
            return strAccount.Length switch
            {
                3 => AccountDetailDto.Create(strAccount[1], url.AbsoluteUri, strAccount[2]),
                5 => AccountDetailDto.Create(strAccount[1], url.AbsoluteUri, strAccount[2], strAccount[3], int.Parse(strAccount[4])),
                7 => AccountDetailDto.Create(strAccount[1], url.AbsoluteUri, strAccount[2], strAccount[3], int.Parse(strAccount[4]), strAccount[5], strAccount[6]),
                _ => null,
            }; ;
        }
    }
}