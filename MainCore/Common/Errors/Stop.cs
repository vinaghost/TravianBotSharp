using FluentResults;

namespace MainCore.Common.Errors
{
    public class Stop : Error
    {
        public Stop(string message) : base($"{message}. Bot must stop")
        {
        }

        public static Stop EnglishRequired(string strType) => new($"Cannot parse {strType}. Is language English ?");

        public static Stop TravianPage => new($"Travian is not ingame nor login page. Please check browser");
    }
}