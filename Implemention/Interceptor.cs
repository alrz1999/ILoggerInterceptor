using ILoggerInterceptor.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading;
using Elastic.Apm.Api;
using Elastic.Apm;

namespace ILoggerInterceptor.Implemention
{
    class Interceptor : IInterceptor
    {
        private readonly string DefaultTransactionType = ApiConstants.ActionExec;
        private readonly string DefaultTransactionName = "Not Defined";
        private readonly string DefaultSpanType = ApiConstants.ActionExec;
        private readonly string DefaultSpanName = "Not Defined";

        private ILogger Logger { get; }
        public Interceptor(ILogger logger)
        {
            Logger = logger;
        }

        
        public IDisposable BeginScope<TState>(TState state)
        {
            if(Agent.Tracer.CurrentTransaction == null)
            {
                StartTransaction(state);
            }
            else
            {
                StartSpan(state);
            }
            var loggerScope = Logger.BeginScope(state);
            return new Scope(loggerScope);
        }

        private void StartSpan<TState>(TState state)
        {
            if(Agent.Tracer.CurrentSpan == null)
            {
                StartSpan(Agent.Tracer.CurrentTransaction, state);
            }
            else
            {
                StartSpan(Agent.Tracer.CurrentSpan, state);
            }
        }

        private void StartSpan<TState>(IExecutionSegment currentexecutionSegment, TState state)
        {
            if (state is string)
            {
                currentexecutionSegment.StartSpan(state.ToString(), DefaultSpanType);
            }
            else if (state is IEnumerable<KeyValuePair<string, object>> Properties)
            {
                var propDic = new Dictionary<string, object>();
                foreach (var item in Properties)
                {
                    if (item.Key != "{OriginalFormat}" && !propDic.ContainsKey(item.Key))
                        propDic.Add(item.Key, item.Value);
                }

                string apmSpanType = propDic.ContainsKey(nameof(apmSpanType)) ? propDic[nameof(apmSpanType)].ToString() : DefaultSpanType;
                string apmSpanName = propDic.ContainsKey(nameof(apmSpanName)) ? propDic[nameof(apmSpanName)].ToString() : DefaultSpanName;
                currentexecutionSegment.StartSpan(apmSpanName, apmSpanType);

                foreach (var item in Properties)
                {
                    currentexecutionSegment.Labels.Add(item.Key, item.Value.ToString());
                }
            }
        }

        private void StartTransaction<TState>(TState state)
        {
            if (state is string)
            {
                Agent.Tracer.StartTransaction(state.ToString(), DefaultTransactionType);
            }
            else if (state is IEnumerable<KeyValuePair<string, object>> Properties)
            {
                var propDic = new Dictionary<string, object>();
                foreach (var item in Properties)
                {
                    if (item.Key != "{OriginalFormat}" && !propDic.ContainsKey(item.Key))
                        propDic.Add(item.Key, item.Value);
                }
                string apmTransactionType = propDic.ContainsKey(nameof(apmTransactionType)) ? propDic[nameof(apmTransactionType)].ToString() : DefaultTransactionType;
                string apmTransactionName = propDic.ContainsKey(nameof(apmTransactionName)) ? propDic[nameof(apmTransactionName)].ToString() : DefaultTransactionName;
                Agent.Tracer.StartTransaction(apmTransactionName, apmTransactionType);
                foreach (var item in Properties)
                {
                    if (item.Key != "{OriginalFormat}" && !propDic.ContainsKey(item.Key))
                        Agent.Tracer.CurrentTransaction.Custom.Add(item.Key, item.Value.ToString());
                }
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logger.Log(logLevel, eventId, state, exception, formatter);

            if(Agent.Tracer.CurrentSpan != null)
            {
                AddMetadata(Agent.Tracer.CurrentSpan, state);
            }
            else if(Agent.Tracer.CurrentTransaction != null)
            {
                AddMetadata(Agent.Tracer.CurrentTransaction, state);
            }
        }

        private static void AddMetadata<TState>(IExecutionSegment executionSegment,TState state)
        {
            if (state is IEnumerable<KeyValuePair<string, object>> Properties)
            {
                foreach (var item in Properties)
                {
                    if (item.Key != "{OriginalFormat}")
                        executionSegment.Labels.Add(item.Key, item.Value.ToString());
                }
            }
        }
    }
}
