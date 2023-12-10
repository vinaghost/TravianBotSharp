using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public class ToBuildingCommand : ByAccountIdBase, ICommand
    {
        public int Location { get; }

        public ToBuildingCommand(AccountId accountId, int location) : base(accountId)
        {
            Location = location;
        }
    }
}