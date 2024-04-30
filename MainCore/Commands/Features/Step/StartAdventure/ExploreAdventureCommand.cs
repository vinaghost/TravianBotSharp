using MainCore.Commands.Base;
using MainCore.Common.MediatR;

namespace MainCore.Commands.Features.Step.StartAdventure
{
    public class ExploreAdventureCommand : ByAccountIdBase, ICommand
    {
        public ExploreAdventureCommand(AccountId accountId) : base(accountId)
        {
        }
    }
}