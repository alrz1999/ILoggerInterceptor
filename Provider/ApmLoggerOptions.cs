using Elastic.Apm.Api;
using Microsoft.Extensions.Logging;

namespace ILoggerInterceptor.Provider
{
    class ApmLoggerOptions
    {
        public LogLevel MinimumLogLevel { get; set; }
        public string ApmMetaDataSign { get; set; } = "Apm";

        public string DefaultTransactionType = ApiConstants.ActionExec;
        public string DefaultTransactionName = "Not Defined";
        public string DefaultSpanType = ApiConstants.ActionExec;
        public string DefaultSpanName = "Not Defined";
        internal static ApmLoggerOptions CreateDefaultApmLoggerOptions()
        {
            return new ApmLoggerOptions()
            {
                MinimumLogLevel = LogLevel.Information
            };
        }
    }
}
