﻿using FluentResults;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.ViewModels.UserControls;
using MediatR;

namespace MainCore.Commands.UI.MainLayout
{
    public class LoginAccountCommand : ByListBoxItemBase, IRequest
    {
        public LoginAccountCommand(ListBoxItemViewModel items) : base(items)
        {
        }
    }

    public class LoginAccountCommandHandler : IRequestHandler<LoginAccountCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly ITimerManager _timerManager;
        private readonly IDialogService _dialogService;

        private readonly IUnitOfRepository _unitOfRepository;

        private readonly IChooseAccessCommand _chooseAccessCommand;
        private readonly IOpenBrowserCommand _openBrowserCommand;
        private readonly ILogService _logService;
        private readonly IMediator _mediator;

        public LoginAccountCommandHandler(ITaskManager taskManager, ITimerManager timerManager, IOpenBrowserCommand openBrowserCommand, IChooseAccessCommand chooseAccessCommand, ILogService logService, IMediator mediator, IDialogService dialogService, IUnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _timerManager = timerManager;
            _openBrowserCommand = openBrowserCommand;
            _chooseAccessCommand = chooseAccessCommand;
            _logService = logService;
            _mediator = mediator;
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(LoginAccountCommand request, CancellationToken cancellationToken)
        {
            var accounts = request.Items;
            if (!accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(accounts.SelectedItemId);

            var tribe = (TribeEnums)_unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.Tribe);
            if (tribe == TribeEnums.Any)
            {
                _dialogService.ShowMessageBox("Warning", "Choose tribe first");
                return;
            }

            if (_taskManager.GetStatus(accountId) != StatusEnums.Offline)
            {
                _dialogService.ShowMessageBox("Warning", "Account's browser is already opened");
                return;
            }

            _taskManager.SetStatus(accountId, StatusEnums.Starting);

            Result result;
            result = await _chooseAccessCommand.Execute(accountId, true);

            if (result.IsFailed)
            {
                _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
                _taskManager.SetStatus(accountId, StatusEnums.Offline);
                return;
            }
            var logger = _logService.GetLogger(accountId);
            var access = _chooseAccessCommand.Value;
            logger.Information("Using connection {proxy} to start chrome", access.Proxy);
            result = await _openBrowserCommand.Execute(accountId, access);
            if (result.IsFailed)
            {
                _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
                _taskManager.SetStatus(accountId, StatusEnums.Offline);
                return;
            }
            await _mediator.Publish(new AccountInit(accountId), cancellationToken);

            _timerManager.Start(accountId);
            _taskManager.SetStatus(accountId, StatusEnums.Online);
        }
    }
}