using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;

namespace MainCore.Commands.Features.Step.DisableContextualHelp
{
    public class ValidateContextualHelpCommand : ByAccountIdBase, ICommand<bool>
    {
        public ValidateContextualHelpCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class ValidateContextualHelpCommandHandler : ICommandHandler<ValidateContextualHelpCommand, bool>
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public bool Value { get; private set; }

        public ValidateContextualHelpCommandHandler(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ValidateContextualHelpCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            Value = _unitOfParser.OptionPageParser.IsContextualHelpShow(html);

            return Result.Ok();
        }
    }
}