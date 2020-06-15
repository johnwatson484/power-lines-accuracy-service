using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PowerLinesAccuracyService.Data;
using System.Linq;
using PowerLinesAccuracyService.Models;
using Microsoft.EntityFrameworkCore;
using PowerLinesAccuracyService.Accuracy;

namespace PowerLinesAccuracyService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccuracyController : ControllerBase
    {
        IAccuracyApi accuracyApi;

        public AccuracyController(IAccuracyApi accuracyApi)
        {
            this.accuracyApi = accuracyApi;
        }

        public ActionResult<IEnumerable<AccuracyResponse>> Get()
        {
            return accuracyApi.Get();
        }
    }
}
