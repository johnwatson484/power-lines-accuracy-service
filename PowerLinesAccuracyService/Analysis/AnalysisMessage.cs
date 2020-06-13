using PowerLinesAccuracyService.Models;

namespace PowerLinesAccuracyService.Analysis
{
    public class AnalysisMessage
    {
        public Fixture Fixture { get; set; }

        public string Sender { get; set; }

        public AnalysisMessage(Fixture fixture)
        {
            Fixture = fixture;
            Sender = "power-lines-accuracy-service";
        }
    }
}
