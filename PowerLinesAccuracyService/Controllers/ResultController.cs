using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PowerLinesAccuracyService.Data;
using System.Linq;
using PowerLinesAccuracyService.Models;
using Microsoft.EntityFrameworkCore;

namespace PowerLinesAccuracyService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResultController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public ResultController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [Route("[action]")]
        public ActionResult<LastResultDate> LastResultDate()
        {
            var date = dbContext.Results.AsNoTracking().OrderByDescending(x => x.Created).FirstOrDefault()?.Created;

            return new LastResultDate(date);
        }
    }
}
