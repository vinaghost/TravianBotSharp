using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;

namespace MainCore.Commands.Validate
{
    public class ValidateAdventureCommand : ByAccountIdBase, ICommand<bool>
    {
        public ValidateAdventureCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class ValidateAdventureCommandHandler : ICommandHandler<ValidateAdventureCommand, bool>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public ValidateAdventureCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public bool Value { get; private set; }

        public async Task<Result> Handle(ValidateAdventureCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);

            var html = chromeBrowser.Html;
            Value = _unitOfParser.HeroParser.CanStartAdventure(html);
            return Result.Ok();
        }
    }
}