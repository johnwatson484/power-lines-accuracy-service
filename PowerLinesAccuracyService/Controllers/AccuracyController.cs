using Microsoft.AspNetCore.Mvc;
using PowerLinesAccuracyService.Accuracy;

namespace PowerLinesAccuracyService.Controllers;

[ApiController]
[Route("[controller]")]
public class AccuracyController(IAccuracyApi accuracyApi) : ControllerBase
{
    readonly IAccuracyApi accuracyApi = accuracyApi;

    [Route("")]
    [HttpGet]
    public ActionResult<IEnumerable<AccuracyResponse>> Index()
    {
        return accuracyApi.Get();
    }
}
