namespace MainCore.Common.Errors.AutoBuilder
{
    public sealed class WaitResource(TimeSpan time) : Error($"Don't have enough resource, waiting for {time.TotalHours} hours")
    {
        public TimeSpan Time { get; } = time;

        public static WaitResource Error(TimeSpan time) => new(time);
    }
}