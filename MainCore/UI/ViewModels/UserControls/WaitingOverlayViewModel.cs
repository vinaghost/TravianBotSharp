﻿using MainCore.UI.ViewModels.Abstract;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace MainCore.UI.ViewModels.UserControls
{
    [RegisterSingleton<IWaitingOverlayViewModel, WaitingOverlayViewModel>]
    public partial class WaitingOverlayViewModel : ViewModelBase, IWaitingOverlayViewModel
    {
        [RegisterServices]
        public static void Register(IServiceCollection services)
        {
            services
                .AddSingleton(x => x.GetService<IWaitingOverlayViewModel>() as WaitingOverlayViewModel);
        }

        public WaitingOverlayViewModel()
        {
            Message = "is initializing";
        }

        public async Task Show(string message)
        {
            await Observable.Start(() =>
            {
                Message = message;
                Shown = true;
            }, RxApp.MainThreadScheduler);
        }

        public async Task Show()
        {
            await Observable.Start(() =>
            {
                Shown = true;
            }, RxApp.MainThreadScheduler);
        }

        public async Task Hide()
        {
            await Observable.Start(() =>
            {
                Shown = false;
                Message = "is initializing";
            }, RxApp.MainThreadScheduler);
        }

        public async Task ChangeMessage(string message)
        {
            await Observable.Start(() =>
            {
                Message = message;
            }, RxApp.MainThreadScheduler);
        }

        [Reactive]
        private bool _shown;

        private string _message;

        public string Message
        {
            get => _message;
            set
            {
                var formattedValue = string.IsNullOrWhiteSpace(value) ? value : $"TBS is {value} ...";
                this.RaiseAndSetIfChanged(ref _message, formattedValue);
            }
        }
    }
}