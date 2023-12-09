using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;

namespace MainCore.Commands.Features.Step.UpgradeBuilding
{
    public class SpecialUpgradeCommand : ByAccountIdBase, ICommand
    {
        public SpecialUpgradeCommand(AccountId accountId) : base(accountId)
        {
        }
    }
}