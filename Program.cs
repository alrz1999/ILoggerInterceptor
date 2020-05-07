using Elastic.Apm;
using ILoggerInterceptor.Implemention;
using ILoggerInterceptor.Provider;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ILoggerInterceptor
{
    class Program
    {
        public static ILogger Logger { get; set; }
        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder
                                                        .AddDebug()
                                                        .AddApm(config => config.MinimumLogLevel = LogLevel.Information));
            Logger = loggerFactory.CreateLogger(nameof(ILoggerInterceptor));
            //Logger = new Interceptor(Logger);
            Test2();
            Thread.Sleep(15000);
        }

        private static void Test2()
        {
            var thread = new Thread(new ThreadStart(TransactionTest));
            thread.Start();
            using (Logger.BeginScope("{apmTransactionName}", "TransactionTest2"))
            {
                Logger.LogInformation("{custom}", 2);
                using (Logger.BeginScope("{apmSpanName}", "SpanTest2"))
                {
                    var spnaThread = new Thread(new ThreadStart(SpanTest));
                    spnaThread.Start();
                    Logger.LogInformation("{custom}", 4);
                }
            }
        }

        private static void Test1()
        {
            using (Logger.BeginScope("{apmTransactionName} {test}", nameof(ILoggerInterceptor), "rest"))
            {
                Logger.LogInformation("this is test log");
                using (Logger.BeginScope("{apmSpanName}", nameof(ILoggerInterceptor)))
                {
                    Logger.LogInformation("this is test log");
                }
            }
        }

        static void TransactionTest()
        {
            using (Logger.BeginScope("{apmTransactionName}", "TransactionTest1"))
            {
                Logger.LogInformation("{custom}",1);
                Thread.Sleep(5000);
            }
        }
        static void SpanTest()
        {
            using (Logger.BeginScope("{apmSpanName}", "SpanTest1"))
            {
                Logger.LogInformation("{custom}", 3);
                Thread.Sleep(5000);
            }
        }
    }
}
