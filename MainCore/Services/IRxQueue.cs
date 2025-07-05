using MainCore.Constraints;

namespace MainCore.Services
{
    public interface IRxQueue
    {
        void Enqueue(INotification notification);

        void RegisterCommand<T>(ReactiveCommand<T, Unit> command) where T : INotification;

        void RegisterHandler<T>(Action<T> handleAction) where T : INotification;
    }
}