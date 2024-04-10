using Discord;
using Discord.Net;
using Discord.Webhook;
using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using Serilog;
using System.Reactive;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class AlertViewModel : AccountTabViewModelBase
    {
        public ReactiveCommand<Unit, Unit> Test { get; }
        public ReactiveCommand<AccountId, string> LoadDiscordWebhookUrl { get; }
        private DiscordWebhookClient _discordWebhookClient;
        private readonly IDialogService _dialogService;
        private readonly UnitOfRepository _unitOfRepository;

        protected override async Task Load(AccountId accountId)
        {
            await LoadDiscordWebhookUrl.Execute(accountId);
        }

        private bool _discordEnable;
        private string _discordWebhookUrl;

        public AlertViewModel(IDialogService dialogService, UnitOfRepository unitOfRepository)
        {
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;

            Test = ReactiveCommand.CreateFromTask(TestHandler);
            LoadDiscordWebhookUrl = ReactiveCommand.Create<AccountId, string>(LoadDiscordWebhookUrlHandler);

            LoadDiscordWebhookUrl.Subscribe(x => DiscordWebhookUrl = x);
        }

        private string LoadDiscordWebhookUrlHandler(AccountId accountId)
        {
            return _unitOfRepository.AccountInfoRepository.GetDiscordWebhookUrl(accountId);
        }

        private async Task TestHandler()
        {
            try
            {
                _discordWebhookClient = new(_discordWebhookUrl);
                var embed = new EmbedBuilder
                {
                    Title = "Server: https://travian.com/ ",
                    Description = "Username: abcxyz\nVillage: abx (21|213)",
                };
                for (var i = 0; i < 20; i++)
                {
                    if (i % 2 == 0)
                    {
                        embed.AddField(new EmbedFieldBuilder()
                        {
                            Name = "[toprakzemin] toprakzemin Köyü (29 | 43)",
                            Value = "attacks 4 waves at 22:08:25 09/04/2024 (and 2 seconds)",
                        });
                    }
                    else
                    {
                        embed.AddField(new EmbedFieldBuilder()
                        {
                            Name = "[xShou] New village (13 | 39)",
                            Value = "**[NEW]** attacks **5** waves at 22:08:20 09/04/2024 (and 1 seconds)",
                        });
                    }
                }

                await _discordWebhookClient.SendMessageAsync(text: "@here", embeds: new[] { embed.Build() });
            }
            catch (ArgumentException)
            {
                _dialogService.ShowMessageBox("Warning", "The given webhook Url was not in a vaild format");
                Log.Error("The given webhook Url was not in a vaild format");
            }
            catch (HttpException error)
            {
                _dialogService.ShowMessageBox(error.Message, error.HelpLink);
                Log.Error("{Message} {HelpLink}", error.Message, error.HelpLink);
            }
            catch (InvalidOperationException error)
            {
                _dialogService.ShowMessageBox("Warning", error.Message);
                Log.Error("{Message}", error.Message);
            }
            catch (Exception error)
            {
                _dialogService.ShowMessageBox("Warning", error.Message);
                Log.Error("{Message}", error.Message);
            }

            var settings = new Dictionary<AccountSettingEnums, int> {
                { AccountSettingEnums.EnableDiscordAlert, DiscordEnable ? 1 : 0 }
            };
            _unitOfRepository.AccountSettingRepository.Update(AccountId, settings);
            _unitOfRepository.AccountInfoRepository.SetDiscordWebhookUrl(AccountId, DiscordWebhookUrl);
        }

        public bool DiscordEnable
        {
            get => _discordEnable;
            set => this.RaiseAndSetIfChanged(ref _discordEnable, value);
        }

        public string DiscordWebhookUrl
        {
            get => _discordWebhookUrl;
            set => this.RaiseAndSetIfChanged(ref _discordWebhookUrl, value);
        }
    }
}