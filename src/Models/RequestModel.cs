namespace MonkeyCage.Models
{
    public class RequestModel
    {
        public string? TargetText { get; set; }
        public TimeSpan Timeout { get; set; }
        public int MonkeyCount { get; set; }
        public bool SaveToDatabase { get; set; }
    }
}
