using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
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
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        public ReactiveCommand<Unit, Unit> AddAccount { get; }
        private ReactiveCommand<string, Unit> UpdateTable { get; }

        public ObservableCollection<AccountDetailDto> Accounts { get; } = new();
        private string _input;

        public string Input
        {
            get => _input;
            set => this.RaiseAndSetIfChanged(ref _input, value);
        }

        public AddAccountsViewModel(IDialogService dialogService, IMediator mediator, WaitingOverlayViewModel waitingOverlayViewModel, IUnitOfRepository unitOfRepository)
        {
            _mediator = mediator;
            _dialogService = dialogService;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _unitOfRepository = unitOfRepository;

            AddAccount = ReactiveCommand.CreateFromTask(AddAccountHandler);
            UpdateTable = ReactiveCommand.CreateFromTask<string>(UpdateTableHandler);

            this.WhenAnyValue(x => x.Input)
                .InvokeCommand(UpdateTable);
        }

        private async Task UpdateTableHandler(string input)
        {
            var data = await Observable.Start(() =>
            {
                return Parse(input);
            }, RxApp.TaskpoolScheduler);
            await Observable.Start(() =>
            {
                Accounts.Clear();
                data.ForEach(Accounts.Add);
            }, RxApp.MainThreadScheduler);
        }

        private async Task AddAccountHandler()
        {
            await _waitingOverlayViewModel.Show("adding accounts");
            await Observable.Start(() =>
            {
                var accounts = Accounts.ToList();
                _unitOfRepository.AccountRepository.Add(accounts);
            }, RxApp.TaskpoolScheduler);

            await _mediator.Publish(new AccountUpdated());

            await Observable.Start(() =>
            {
                Accounts.Clear();
                Input = "";
            }, RxApp.MainThreadScheduler);

            await _waitingOverlayViewModel.Hide();

            _dialogService.ShowMessageBox("Information", "Added accounts");
        }

        private static List<AccountDetailDto> Parse(string input)
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