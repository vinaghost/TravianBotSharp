namespace MainCore.Common.Models
{
    public class CelebrationPlan
    {
        public bool Big { get; set; }

        public override string ToString()
        {
            var celebration = Big ? "Great celebration" : "Small celebration";
            return $"Hold {celebration}";
        }
    }
}