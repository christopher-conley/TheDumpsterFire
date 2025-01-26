using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RosettaTools.Pwsh.Text.RevenantLogger;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common {
    public sealed class SuccessMessage : RevenantLoggerBase  {
        public static readonly SuccessMessage Value = new();

        private SuccessMessage() { }

        public override string ToString() => "SUCCESS";
    }

    public sealed class WarnMessage : RevenantLoggerBase
    {
        public static readonly WarnMessage Value = new();

        private WarnMessage() { }

        public override string ToString() => "WARNING";
    }

    public sealed class FailMessage : RevenantLoggerBase
    {
        public static readonly FailMessage Value = new();

        private FailMessage() { }

        public override string ToString() => "FAILURE";
    }



    public sealed class Sep : RevenantLoggerBase
    {
        public static readonly Sep Value = new();

        private Sep() { }

        public override string ToString() => $"{RevenantConfig.LoggingConfig.DateTimeSeperator}";
    }


    public sealed class LogDate : RevenantLoggerBase
    {
        public static readonly LogDate Value = new();
        private DateTime calledDate;

        private LogDate() {
            //if (RevenantConfig.LoggingConfig.UTC)
            //{
            //    calledDate = DateTime.UtcNow;
            //}
            //else
            //{
            //    calledDate = DateTime.Now;
            //}
        }

        public override string ToString()
        {
            //if (RevenantConfig.LoggingConfig.UTC)
            //{
            //    calledDate = DateTime.UtcNow;
            //}
            //else
            //{
            //    calledDate = DateTime.Now;
            //}

            //string outputDate = calledDate.ToString(RevenantConfig.LoggingConfig.TimeFormat);
            //return $"[dim cyan]{outputDate}[/]";
            return $"[dim cyan]";
        }
    }

    public sealed class LogTime : RevenantLoggerBase
    {
        public static LogTime Value = new();
        private DateTime calledTime;

        private LogTime()
        {
            //if (RevenantConfig.LoggingConfig.UTC)
            //{
            //    calledTime = DateTime.UtcNow;
            //}
            //else
            //{
            //    calledTime = DateTime.Now;
            //}
        }

        public override string ToString()
        {
            //if (RevenantConfig.LoggingConfig.UTC)
            //{
            //    calledTime = DateTime.UtcNow;
            //}
            //else
            //{
            //    calledTime = DateTime.Now;
            //}

            //string outputTime = calledTime.ToString(RevenantConfig.LoggingConfig.TimeFormat);
            //return $"[cyan]{outputTime}[/]";
            return $"[cyan]";
        }
    }

    //public sealed class FormattedTimestamp : RevenantLoggerBase
    //{
    //    public static readonly FormattedTimestamp Value = new();

    //    private FormattedTimestamp() { }

    //    public override string ToString()
    //    {
    //        return $"{LogDate.Value}{RevenantConfig.LoggingConfig.DateTimeSeperator}{LogTime.Value}";
    //    }
    //}

    //public sealed class SepColor : RevenantLoggerBase
    //{
    //    public static readonly SepColor Value = new();

    //    private SepColor() { }

    //    public override string ToString() => "[bold grey][[[/]";
    //}

    public sealed class OpenBracket : RevenantLoggerBase
    {
        public static readonly OpenBracket Value = new();

        private OpenBracket() { }

        public override string ToString() => "[bold grey][[[/]";
    }

    public sealed class CloseBracket : RevenantLoggerBase
    {
        public static readonly CloseBracket Value = new();

        private CloseBracket() { }

        public override string ToString() => "[bold grey]]][/]";
    }

    public class FormattedDate : RevenantLoggerBase
    {
        public static FormattedDate Value = new();
        public FormattedDate()
        {
            
        }

        public override string ToString()
        {
            DateTime calledDate;
            if (RevenantConfig.LoggingConfig.UTC)
            {
                calledDate = DateTime.UtcNow;
            }
            else
            {
                calledDate = DateTime.Now;
            }
            return calledDate.ToString(RevenantConfig.LoggingConfig.DateFormat);

            //string outputDate = calledDate.ToString(RevenantConfig.LoggingConfig.DateFormat);
            //return $"[dim cyan]{outputDate}[/]";
        }
    }

    public class FormattedTime : RevenantLoggerBase
    {
        public static FormattedTime Value = new();
        public FormattedTime()
        {

        }

        public override string ToString()
        {
            DateTime calledTime;
            if (RevenantConfig.LoggingConfig.UTC)
            {
                calledTime = DateTime.UtcNow;
            }
            else
            {
                calledTime = DateTime.Now;
            }

            return calledTime.ToString(RevenantConfig.LoggingConfig.TimeFormat);

            //string outputDate = calledTime.ToString(RevenantConfig.LoggingConfig.TimeFormat);
            //return $"[cyan]{outputDate}[/]";
        }
    }

}
