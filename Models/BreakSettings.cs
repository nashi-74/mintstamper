namespace mintstamper.Models
{
    public partial class BreakSettings
    {
        public string? Interval { get; set; }
        public bool Enabled { get; set; }

        public BreakSettings(string? interval, bool enabled)
        {
            Interval = interval;
            Enabled = enabled;
        }
    }
}
