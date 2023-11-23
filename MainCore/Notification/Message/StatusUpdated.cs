using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Notification.Message
{
    public class StatusUpdated : ByAccountIdBase, INotification
    {
        public StatusEnums Status { get; }

        public StatusUpdated(AccountId accountId, StatusEnums status) : base(accountId)
        {
            Status = status;
        }
    }
}