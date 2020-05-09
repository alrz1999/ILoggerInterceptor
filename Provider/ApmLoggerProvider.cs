using Microsoft.Extensions.Logging;
using System;

namespace ILoggerInterceptor.Provider
{
    [ProviderAlias("Apm")]
    public class ApmLoggerProvider : ILoggerProvider
    {
        internal ApmLoggerOptions ApmConfigs;

        public bool IsDisposed { get; private set; }

        internal ApmLoggerProvider(Action<ApmLoggerOptions> configure = null)
        {
            ApmConfigs = ApmLoggerOptions.CreateDefaultApmLoggerOptions();
            configure?.Invoke(ApmConfigs);
        }

        internal ApmLoggerProvider(ApmLoggerOptions configuration)
        {
            ApmConfigs = configuration;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ApmLogger(categoryName,ApmConfigs);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public void SetMinimumLevel()
        {
                
        }
    }
}
