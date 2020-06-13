using System;
using System.Threading.Tasks;

namespace PowerLinesAccuracyService.Analysis
{
    public interface IAnalysisApi
    {
        Task<DateTime?> GetLastResultDate();
    }
}
