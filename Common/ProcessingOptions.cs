namespace Common
{
    public class ProcessingOptions
    {
        public bool EnableLogging { get; set; } = true;
        public int MaxRetries { get; set; } = 3;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public string ProcessingMode { get; set; } = "Standard";
        public Dictionary<string, string> CustomSettings { get; set; } = new Dictionary<string, string>();
    }
} 