using Elastic.Apm;
using Elastic.Apm.Api;
using ILoggerInterceptor.Implemention;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ILoggerInterceptor.Provider
{
    class ApmLogger : ILogger
    {
        private static readonly string DefaultTransactionType = ApiConstants.ActionExec;
        private static readonly string DefaultTransactionName = "Not Defined";
        private static readonly string DefaultSpanType = ApiConstants.ActionExec;
        private static readonly string DefaultSpanName = "Not Defined";

        private readonly string categoryName;
        private ApmLoggerOptions ApmOptions { get; }


        public ApmLogger(string categoryName, ApmLoggerOptions apmConfigs)
        {
            this.categoryName = categoryName;
            ApmOptions = apmConfigs;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (Agent.Tracer.CurrentTransaction == null)
            {
                StartTransaction(state);
            }
            else
            {
                StartSpan(state);
            }
            return new Scope();
        }

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel != LogLevel.None &&
            ApmOptions.MinimumLogLevel != LogLevel.None &&
            Convert.ToInt32(logLevel) >= Convert.ToInt32(ApmOptions.MinimumLogLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (Agent.Tracer.CurrentSpan != null)
            {
                AddMetadata(Agent.Tracer.CurrentSpan, state);
            }
            else if (Agent.Tracer.CurrentTransaction != null)
            {
                AddMetadata(Agent.Tracer.CurrentTransaction, state);
            }
        }
        private void StartSpan<TState>(TState state)
        {
            if (Agent.Tracer.CurrentSpan == null)
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

        private static void AddMetadata<TState>(IExecutionSegment executionSegment, TState state)
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
