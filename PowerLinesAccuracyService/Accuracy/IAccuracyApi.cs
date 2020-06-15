using System.Collections.Generic;

namespace PowerLinesAccuracyService.Accuracy
{
    public interface IAccuracyApi
    {
        List<AccuracyResponse> Get();
    }
}
