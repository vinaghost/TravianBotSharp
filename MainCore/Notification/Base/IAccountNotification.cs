namespace MainCore.Notification.Base
{
    public interface IAccountNotification : INotification
    {
        AccountId AccountId { get; }
    }
}