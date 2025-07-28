namespace FreelanceJobBoard.API.Attributes
{
    public class RateLimitAttribute : Attribute
    {
        public int MaxRequests { get; set; }
        public TimeSpan Window { get; set; }
        public RateLimitAttribute(int maxRequests, int windowSeconds) => (MaxRequests, Window) = (maxRequests, TimeSpan.FromSeconds(windowSeconds));
    }
}
