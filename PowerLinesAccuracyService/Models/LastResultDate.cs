using System;

namespace PowerLinesAccuracyService.Models
{
    public class LastResultDate
    {
        public bool Available { get; set; }

        public DateTime? Date { get; set; }

        public LastResultDate(DateTime? date)
        {
            Available = date != null;
            Date = date;
        }
    }
}
