using Microsoft.Extensions.Logging;
using System;

namespace ILoggerInterceptor.Provider
{
    static class ApmLoggerExtensions
    {
        public static ILoggingBuilder AddApm(this ILoggingBuilder builder)
        {
            return builder.AddProvider(new ApmLoggerProvider());
        }

        public static ILoggingBuilder AddApm(this ILoggingBuilder builder,Action<ApmLoggerOptions> configure)
        {
            return builder.AddProvider(new ApmLoggerProvider(configure));
        }
    }
}
