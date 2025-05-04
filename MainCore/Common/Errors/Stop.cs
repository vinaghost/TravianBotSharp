namespace MainCore.Common.Errors
{
    public class Stop : Error
    {
        private Stop(string message) : base($"{message}. Bot must stop")
        {
        }

        public static Stop EnglishRequired(string strType) => new($"Cannot parse {strType}. Is language English ?");

        public static Stop NotTravianPage => new($"Travian is not ingame nor login page. Please check browser");

        public static Stop NotEnoughStorageCapacity => new("Please take a look on building job queue");

        public static Stop AllAccessNotWorking => new("All accesses not working");
        public static Stop LackOfAccess => new("Last access is reused, it may get MH's attention");
    }
}