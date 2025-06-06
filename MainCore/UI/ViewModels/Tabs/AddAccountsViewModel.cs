﻿using MainCore.Commands.UI.AddAccountsViewModel;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<AddAccountsViewModel>]
    public partial class AddAccountsViewModel : TabViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public ObservableCollection<AccountDetailDto> Accounts { get; } = [];

        [Reactive]
        private string _input = "";

        public AddAccountsViewModel(IDialogService dialogService, IWaitingOverlayViewModel waitingOverlayViewModel, IServiceScopeFactory serviceScopeFactory)
        {
            _dialogService = dialogService;
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _serviceScopeFactory = serviceScopeFactory;

            this.WhenAnyValue(x => x.Input)
                .ObserveOn(RxApp.TaskpoolScheduler)
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
            await _waitingOverlayViewModel.Show("adding accounts");

            using var scope = _serviceScopeFactory.CreateScope();
            var addAccountsCommand = scope.ServiceProvider.GetRequiredService<AddAccountsCommand.Handler>();
            var resultInput = await addAccountsCommand.HandleAsync(new([.. Accounts.Select(x => x.ToDto())]));

            await _waitingOverlayViewModel.Hide();

            if (resultInput.IsFailed)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", resultInput.Errors[0].Message));
                return;
            }
            await _dialogService.MessageBox.Handle(new MessageBoxData("Information", $"Added accounts"));
            await _waitingOverlayViewModel.Hide();
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
    }
}