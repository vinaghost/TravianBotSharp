namespace MainCore.Common.Errors.AutoBuilder
{
    public sealed class WaitResource : Error
    {
        private WaitResource(TimeSpan time)
        {
            Time = time;
        }

        public TimeSpan Time { get; private set; }

        public static WaitResource Error(TimeSpan time) => new(time);
    }
}