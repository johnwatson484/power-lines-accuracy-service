using PowerLinesAccuracyService.Models;

namespace PowerLinesAccuracyService.Analysis;

public class AnalysisMessage(Fixture fixture)
{
    public Fixture Fixture { get; set; } = fixture;

    public string Sender { get; set; } = "power-lines-accuracy-service";
}
