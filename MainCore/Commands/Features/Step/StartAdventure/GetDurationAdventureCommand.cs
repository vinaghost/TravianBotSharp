using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;

namespace MainCore.Commands.Features.Step.StartAdventure
{
    public class GetDurationAdventureCommand : ByAccountIdBase, ICommand<TimeSpan>
    {
        public GetDurationAdventureCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class GetDurationAdventureCommandHandler : ICommandHandler<GetDurationAdventureCommand, TimeSpan>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public GetDurationAdventureCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public TimeSpan Value { get; private set; }

        public async Task<Result> Handle(GetDurationAdventureCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var adventure = _unitOfParser.HeroParser.GetAdventureDuration(html);
            Value = adventure;
            return Result.Ok();
        }
    }
}