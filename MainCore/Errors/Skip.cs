namespace MainCore.Errors
{
    public class Skip : Error
    {
        private Skip(string message) : base(message)
        {
        }

        public static Skip VillageNotFound => new("Village not found");
        public static Skip BuildingJobQueueEmpty => new("Building job queue is empty");
        public static Skip BuildingJobQueueBroken => new("Building job queue is broken. No building in construct but cannot choose job");
        public static Skip NotEnoughResource => new("Reschedule becasue doesn't have enough resource");
        public static Skip ConstructionQueueFull => new("Construction queue is full");
        public static Skip WaitingStorageUpgrade => new("Waiting for storage upgrade to finish");
        public static Skip AccountLogout => new("Account is logout. Re-login now");

        public static Skip NoRallypoint => new("No rallypoint found. Recheck & load village has rallypoint in Village>Build tab");
        public static Skip NoActiveFarmlist => new("No farmlist is active");
        public static Skip NoAdventure => new("No adventure available");

        public static Skip WrongVillage => new("Wrong village active");

        public static Skip GranaryNotReady => new("Granary percent below threshold");
    }
}