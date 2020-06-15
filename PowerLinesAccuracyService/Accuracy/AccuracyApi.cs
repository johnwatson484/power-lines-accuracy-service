using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PowerLinesAccuracyService.Data;

namespace PowerLinesAccuracyService.Accuracy
{
    public class AccuracyApi : IAccuracyApi
    {
        private readonly ApplicationDbContext dbContext;

        public AccuracyApi(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<AccuracyResponse> Get()
        {
            var response = new List<AccuracyResponse>();

            var accuracy = dbContext.Accuracy.AsNoTracking();

            foreach (var calculation in accuracy)
            {
                response.Add(new AccuracyResponse(calculation));
            }

            return response.OrderBy(x => x.CountryRank).ThenBy(x => x.Tier).ToList();
        }
    }
}
