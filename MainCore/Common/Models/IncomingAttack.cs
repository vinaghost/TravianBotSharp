namespace MainCore.Common.Models
{
    public class IncomingAttack
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string PlayerName { get; set; }
        public int WaveCount { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int DelaySecond { get; set; }
    }
}