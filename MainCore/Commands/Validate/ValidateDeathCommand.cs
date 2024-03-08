using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;

namespace MainCore.Commands.Validate
{
    public class ValidateDeathCommand : ByAccountIdBase, ICommand<bool>
    {
        public ValidateDeathCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class ValidateDeathCommandHandler : ICommandHandler<ValidateDeathCommand, bool>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public ValidateDeathCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public bool Value { get; private set; }

        public async Task<Result> Handle(ValidateDeathCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);

            var html = chromeBrowser.Html;
            Value = _unitOfParser.HeroParser.IsDead(html);
            return Result.Ok();
        }
    }
}