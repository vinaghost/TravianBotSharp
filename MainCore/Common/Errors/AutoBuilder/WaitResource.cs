namespace MainCore.Common.Errors.AutoBuilder
{
    public sealed class WaitResource(TimeSpan time) : Error
    {
        public TimeSpan Time { get; } = time;

        public static WaitResource Error(TimeSpan time) => new(time);
    }
}