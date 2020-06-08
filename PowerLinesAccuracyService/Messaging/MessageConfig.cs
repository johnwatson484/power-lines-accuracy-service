using System;
using System.Configuration;

namespace PowerLinesAccuracyService.Messaging
{
    public class MessageConfig
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string ResultQueue { get; set; }

        public string ResultUsername { get; set; }

        public string ResultPassword { get; set; }

        public string AnalysisQueue { get; set; }

        public string AnalysisUsername { get; set; }

        public string AnalysisPassword { get; set; }

        public string OddsQueue { get; set; }

        public string OddsUsername { get; set; }

        public string OddsPassword { get; set; }
    }
}
