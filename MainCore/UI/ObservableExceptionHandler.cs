using MainCore.Infrasturecture.AutoRegisterDi;
using ReactiveUI;
using Serilog;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace MainCore.UI
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class ObservableExceptionHandler : IObserver<Exception>
    {
        public void OnNext(Exception value)
        {
            Handle(value);
        }

        public void OnError(Exception error)
        {
            Handle(error);
        }

        public void OnCompleted()
        {
            Handle(null);
        }

        private static void Handle(Exception exception)
        {
            if (exception is null) return;
            Log.Error(exception, "UI execption");
            if (Debugger.IsAttached)
            {
                Debugger.Break();
                RxApp.MainThreadScheduler.Schedule(() => { throw exception; });
            }
        }
    }
}