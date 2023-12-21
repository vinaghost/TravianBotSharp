using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;

namespace MainCore.Commands.Navigate
{
    public class ToQuestPageCommand : ByAccountIdBase, ICommand
    {
        public ToQuestPageCommand(AccountId accountId) : base(accountId)
        {
        }
    }
}