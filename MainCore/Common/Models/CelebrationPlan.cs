namespace MainCore.Common.Models
{
    public class CelebrationPlan
    {
        public bool Great { get; set; }

        public override string ToString()
        {
            var celebration = Great ? "Great celebration" : "Small celebration";
            return $"Hold {celebration}";
        }
    }
}