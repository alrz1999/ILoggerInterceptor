using Elastic.Apm;
using Elastic.Apm.Api;
using ILoggerInterceptor.Implemention;
using Microsoft.Extensions.Logging;
using System;

namespace ILoggerInterceptor.Extensions
{
    static class LoggerExtensions
    {
        private static Func<ILogger, string, string, IDisposable> _startTransaction;
        private static Func<ILogger, string, string, IDisposable> _startSpan;
        private static Action<ILogger,Exception> _captureException;
        private static Action<ILogger, string, string,Exception> _addMetaData;

        static LoggerExtensions()
        {
            _startTransaction = LoggerMessage.DefineScope<string, string>(null);
            _startSpan = LoggerMessage.DefineScope<string, string>(null);
            _captureException = LoggerMessage.Define(LogLevel.Error, new EventId(1, nameof(CaptureException)),null);
            _addMetaData = LoggerMessage.Define<string,string>(LogLevel.Information, new EventId(2, nameof(AddMetaData)), null);
        }

        public static IDisposable StartTransactionScope(this ILogger logger, string transactionName, string transactionType)
        {
            Agent.Tracer.StartTransaction(transactionName, transactionType);
                
            return new Scope(_startTransaction(logger,transactionName,transactionType));
        }
        public static IDisposable StartSpanScope(this ILogger logger, string spanName, string spanType)
        {
            GetCurrentSpan().StartSpan(spanName, spanType);

            return new Scope(_startSpan(logger, spanName, spanType));
        }
        public static void CaptureException(this ILogger logger,Exception exception)
        {
            GetCurrentSpan().CaptureException(exception);

            _captureException(logger,exception);
        }
        public static void AddMetaData(this ILogger logger,string key,string value, Exception exception = null)
        {
            GetCurrentSpan().Labels.Add(key, value);

            _addMetaData(logger, key, value, exception);
        }

        static private IExecutionSegment GetCurrentSpan()
        {
            if (Agent.Tracer.CurrentSpan == null)
            {
                return Agent.Tracer.CurrentTransaction;
            }
            else
            {
                return Agent.Tracer.CurrentSpan;
            }
        }
    }
}

