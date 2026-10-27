namespace MainCore.Services.AiPlanning
{
    /// <summary>Stops queued UI progress from overwriting an operation's final result.</summary>
    public sealed class PlannerProgress : IProgress<string>, IDisposable
    {
        private readonly IProgress<string> _progress;
        private bool _disposed;

        public PlannerProgress(Action<string> report)
        {
            _progress = new Progress<string>(message =>
            {
                if (!Volatile.Read(ref _disposed)) report(message);
            });
        }

        public void Report(string message) => _progress.Report(message);
        public void Dispose() => Volatile.Write(ref _disposed, true);
    }
}
