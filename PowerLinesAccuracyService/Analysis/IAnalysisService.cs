using System.Collections.Generic;
using PowerLinesAccuracyService.Models;

namespace PowerLinesAccuracyService.Analysis
{
    public interface IAnalysisService
    {
        MatchOdds GetMatchOdds(Fixture fixture, List<Result> historicResults = null);
    }
}
