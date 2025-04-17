namespace MainCore.Common.Extensions
{
    public static class StatusExtension
    {
        public static SplatColor GetColor(this StatusEnums status)
        {
            return status switch
            {
                StatusEnums.Online => SplatColor.Green,
                StatusEnums.Starting => SplatColor.Orange,
                StatusEnums.Pausing => SplatColor.Orange,
                StatusEnums.Stopping => SplatColor.Orange,
                StatusEnums.Offline => SplatColor.Black,
                StatusEnums.Paused => SplatColor.Red,
                _ => SplatColor.Black,
            };
        }
    }
}