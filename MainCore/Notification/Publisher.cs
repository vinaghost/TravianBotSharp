using Immediate.Handlers.Shared;

namespace MainCore.Notification
{
    public class Publisher<TNotification>(IEnumerable<IHandler<TNotification, ValueTuple>> handlers) where TNotification : INotification
    {
        public async ValueTask Publish(TNotification notification, CancellationToken token)
        {
            foreach (var handler in handlers)
            {
                if (token.IsCancellationRequested) return;
                await handler.HandleAsync(notification, token);
            }
        }
    }

    [RegisterSingleton<Publisher>]
    public class Publisher
    {
        public async ValueTask Publish<TNotification>(TNotification notification, CancellationToken token) where TNotification : INotification
        {
            var publisher = Locator.Current.GetService<Publisher<TNotification>>();
            if (publisher != null)
            {
                await publisher.Publish(notification, token);
            }
            else
            {
                throw new InvalidOperationException($"No handlers found for notification type {typeof(TNotification).Name}");
            }
        }
    }
}