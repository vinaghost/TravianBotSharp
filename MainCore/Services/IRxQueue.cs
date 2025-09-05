namespace MainCore.Services
{
    public interface IRxQueue
    {
        void Enqueue(INotification notification);

        IObservable<T> GetObservable<T>() where T : INotification;

        void RegisterCommand<T>(ReactiveCommand<T, Unit> command) where T : INotification;

        void RegisterHandler<T>(Action<T> handleAction) where T : INotification;

        void Setup();
    }
}
