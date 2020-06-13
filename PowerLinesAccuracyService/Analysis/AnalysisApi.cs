using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PowerLinesAccuracyService.Analysis
{
    public class AnalysisApi : IAnalysisApi
    {
        AnalysisUrl analysisUrl;

        public AnalysisApi(AnalysisUrl analysisUrl)
        {
            this.analysisUrl = analysisUrl;
        }

        public async Task<DateTime?> GetLastResultDate()
        {
            DateTime? lastResultDate = null;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(string.Format("{0}/{1}", analysisUrl.Endpoint, analysisUrl.LastResultDate)))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        lastResultDate = JsonConvert.DeserializeObject<LastResultDate>(apiResponse).Date;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Analysis API unavailable: {0}", ex);
            }

            return lastResultDate;
        }
    }
}
