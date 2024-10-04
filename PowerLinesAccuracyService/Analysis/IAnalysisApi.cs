namespace PowerLinesAccuracyService.Analysis;

public interface IAnalysisApi
{
    Task<DateTime?> GetLastResultDate();
}

