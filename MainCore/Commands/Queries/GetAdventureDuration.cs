namespace MainCore.Commands.Queries
{
    public class GetAdventureDuration
    {
        public TimeSpan Execute(IChromeBrowser chromeBrowser)
        {
            var html = chromeBrowser.Html;
            var heroAdventure = html.GetElementbyId("heroAdventure");
            var timer = heroAdventure
                .Descendants("span")
                .Where(x => x.HasClass("timer"))
                .FirstOrDefault();
            if (timer is null) return TimeSpan.Zero;

            var seconds = timer.GetAttributeValue("value", 0);
            return TimeSpan.FromSeconds(seconds);
        }
    }
}