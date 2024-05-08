namespace MainCore.Commands.Abstract
{
    public abstract class AdventureCommand
    {
        public HtmlNode GetHeroAdventure(HtmlDocument doc)
        {
            var adventure = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("adventure") && x.HasClass("round"));
            return adventure;
        }

        public bool CanStartAdventure(HtmlDocument doc)
        {
            var status = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroStatus"));
            if (status is null) return false;

            var heroHome = status.Descendants("i")
                .Any(x => x.HasClass("heroHome"));
            if (!heroHome) return false;

            var adventure = GetHeroAdventure(doc);
            if (adventure is null) return false;

            var adventureAvailabe = adventure.Descendants("div")
                .Any(x => x.HasClass("content"));
            return adventureAvailabe;
        }
    }
}