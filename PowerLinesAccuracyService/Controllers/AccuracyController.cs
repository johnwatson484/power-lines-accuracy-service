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
    public class AccuracyController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public AccuracyController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [Route("[action]")]
        public ActionResult<IEnumerable<Models.Accuracy>> Get()
        {
            return dbContext.Accuracy.OrderBy(x => x.Division).ToList();
        }
    }
}
