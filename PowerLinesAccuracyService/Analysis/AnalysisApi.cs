using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace PowerLinesAccuracyService.Analysis;

public class AnalysisApi(IOptions<AnalysisOptions> analysisOptions) : IAnalysisApi
{
    readonly AnalysisOptions analysisOptions = analysisOptions.Value;

    public async Task<DateTime?> GetLastResultDate()
    {
        DateTime? lastResultDate = null;

        try
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(string.Format("{0}/{1}", analysisOptions.Endpoint, analysisOptions.LastResultDate));
            string apiResponse = await response.Content.ReadAsStringAsync();
            lastResultDate = JsonConvert.DeserializeObject<LastResultDate>(apiResponse).Date;
        }
        catch (Exception)
        {
            Console.WriteLine("Analysis API unavailable");
        }

        return lastResultDate;
    }
}
