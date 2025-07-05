using MainCore.Constraints;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace MainCore.Services
{
    [RegisterSingleton<IRxQueue, RxQueue>]
    public class RxQueue : IRxQueue
    {
        private readonly Subject<INotification> _notifications = new Subject<INotification>();
        private IConnectableObservable<INotification> _connectableObservable;

        private readonly IServiceProvider _serviceProvider;

        public RxQueue(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _connectableObservable = _notifications.ObserveOn(Scheduler.Default).Publish();
            _connectableObservable.Connect();
        }

        public void Enqueue(INotification notification)
        {
            _notifications.OnNext(notification);
        }

        public void RegisterHandler<T>(Action<T> handleAction) where T : INotification
        {
            _connectableObservable.OfType<T>().Subscribe(handleAction);
        }

        public void RegisterCommand<T>(ReactiveCommand<T, Unit> command) where T : INotification
        {
            _connectableObservable.OfType<T>().InvokeCommand(command);
        }
    }
}