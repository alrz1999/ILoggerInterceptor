using Microsoft.Extensions.Logging;

namespace ILoggerInterceptor.Provider
{
    class ApmLoggerOptions
    {
        public LogLevel MinimumLogLevel { get; set; }

        internal static ApmLoggerOptions CreateDefaultApmLoggerOptions()
        {
            return new ApmLoggerOptions()
            {
                MinimumLogLevel = LogLevel.Information
            };
        }
    }
}
