using FluentValidation;
using MainCore.Common.Models;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using System.Reactive.Linq;

namespace MainCore.Commands.UI.Villages
{
    [RegisterSingleton<BuildResourceCommand>]
    public class BuildResourceCommand
    {
        private readonly IMediator _mediator;
        private readonly IDialogService _dialogService;
        private readonly IValidator<ResourceBuildInput> _resourceBuildInputValidator;

        public BuildResourceCommand(IMediator mediator, IDialogService dialogService, IValidator<ResourceBuildInput> resourceBuildInputValidator)
        {
            _mediator = mediator;
            _dialogService = dialogService;
            _resourceBuildInputValidator = resourceBuildInputValidator;
        }

        public async Task Execute(AccountId accountId, VillageId villageId, ResourceBuildInput resourceBuildInput)
        {
            var result = await _resourceBuildInputValidator.ValidateAsync(resourceBuildInput);
            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            var (type, level) = resourceBuildInput.Get();
            var plan = new ResourceBuildPlan()
            {
                Plan = type,
                Level = level,
            };
            var addJobCommand = Locator.Current.GetService<AddJobCommand>();
            addJobCommand.ToBottom(villageId, plan);
            await _mediator.Publish(new JobUpdated(accountId, villageId));
        }
    }
}