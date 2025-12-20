using System;
using System.Collections.Generic;
using System.Text;
using Serilog.Configuration;
using Serilog.Enrichers;

namespace Serilog
{
    public static class CallerNameConfigurationExtension
    {
        public static LoggerConfiguration WithCallerNameEnricher(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<CallerNameEnricher>();
        }
    }
}
