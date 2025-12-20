using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSPhlebotomist.Common
{
    public static class ILoggerExtensions
    {
        public static ILogger ForContext<T>(this ILogger logger)
        {
            return logger.ForContext(Constants.SourceContextPropertyName, typeof(T).Name);
        }
    }
}
