namespace PowerLinesAccuracyService.Messaging;

public class MessageOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string ResultQueue { get; set; }
    public string ResultSubscription { get; set; }
    public string AnalysisQueue { get; set; }
    public string OddsQueue { get; set; }
    public string OddsSubscription { get; set; }
}

