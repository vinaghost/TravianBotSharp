namespace MainCore.Commands.Queries
{
    public class IsIngamePage
    {
        public bool Execute(IChromeBrowser chromeBrowser)
        {
            var html = chromeBrowser.Html;

            var serverTime = html.GetElementbyId("servertime");

            return serverTime is not null;
        }
    }
}