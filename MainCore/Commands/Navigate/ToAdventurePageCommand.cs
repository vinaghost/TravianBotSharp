using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public class ToAdventurePageCommand : ByAccountIdBase, ICommand
    {
        public ToAdventurePageCommand(AccountId accountId) : base(accountId)
        {
        }
    }
}