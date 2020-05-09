using Elastic.Apm;
using Elastic.Apm.Api;
using ILoggerInterceptor.Implemention;
using Microsoft.Extensions.Logging;
using System;

namespace ILoggerInterceptor.Extensions
{
    public static class LoggerExtensions
    {
        private static readonly Func<ILogger, string, string, IDisposable> _startTransaction;
        private static readonly Func<ILogger, string, string, IDisposable> _startSpan;
        private static readonly Action<ILogger,Exception> _captureException;
        private static readonly Action<ILogger, string, string, Exception> _addMetaData;

        static LoggerExtensions()
        {
            _startTransaction = LoggerMessage.DefineScope<string, string>("transaction {TransactionName} with type {TransactionType} started");
            _startSpan = LoggerMessage.DefineScope<string, string>("span {SpanName} with type {SpanType} started");
            _captureException = LoggerMessage.Define(LogLevel.Error, new EventId(1, nameof(CaptureException)),"exception send to Apm");
            _addMetaData = LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(2, nameof(AddMetaDataToCurrentTransaction)), "key:{key}. value:{value}");
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

            _captureException(logger, exception);
        }
        public static void AddMetaDataToCurrentTransaction(this ILogger logger,string key,string value, Exception exception = null)
        {
            Agent.Tracer.CurrentTransaction.Labels.Add(key, value);

            _addMetaData(logger, key, value, exception);
        }

        public static void AddMetaDataToCurrentSpan(this ILogger logger, string key, string value, Exception exception = null)
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

