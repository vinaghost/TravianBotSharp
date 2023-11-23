using MainCore.Common.Enums;
using System.Drawing;

namespace MainCore.Common.Extensions
{
    public static class StatusExtension
    {
        public static Color GetColor(this StatusEnums status)
        {
            return status switch
            {
                StatusEnums.Online => Color.Green,
                StatusEnums.Starting => Color.Orange,
                StatusEnums.Pausing => Color.Orange,
                StatusEnums.Stopping => Color.Orange,
                StatusEnums.Offline => Color.Black,
                StatusEnums.Paused => Color.Red,
                _ => Color.Black,
            };
        }
    }
}