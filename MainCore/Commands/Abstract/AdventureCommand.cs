namespace MainCore.Commands.Abstract
{
    public abstract class AdventureCommand
    {
        public HtmlNode GetHeroAdventure(HtmlDocument doc)
        {
            var adventure = doc.DocumentNode
                .Descendants("a")
                .Where(x => x.HasClass("adventure") && x.HasClass("round"))
                .FirstOrDefault();
            return adventure;
        }

        public bool CanStartAdventure(HtmlDocument doc)
        {
            var status = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("heroStatus"))
                .FirstOrDefault();
            if (status is null) return false;

            var heroHome = status.Descendants("i")
                .Where(x => x.HasClass("heroHome"))
                .Any();
            if (!heroHome) return false;

            var adventure = GetHeroAdventure(doc);
            if (adventure is null) return false;

            var adventureAvailabe = adventure.Descendants("div")
                .Where(x => x.HasClass("content"))
                .Any();
            return adventureAvailabe;
        }
    }
}