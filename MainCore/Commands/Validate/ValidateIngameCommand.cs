using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;

namespace MainCore.Commands.Validate
{
    public class ValidateIngameCommand : ByAccountIdBase, ICommand<bool>
    {
        public ValidateIngameCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class ValidateIngameCommandHandler : ICommandHandler<ValidateIngameCommand, bool>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public ValidateIngameCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public bool Value { get; private set; }

        public async Task<Result> Handle(ValidateIngameCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var fieldButton = _unitOfParser.NavigationBarParser.GetResourceButton(html);

            Value = fieldButton is not null;
            return Result.Ok();
        }
    }
}