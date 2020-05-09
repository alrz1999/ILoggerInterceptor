using ILoggerInterceptor.Extensions;
using ILoggerInterceptor.Provider;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace ILoggerInterceptor
{
    class Program
    {
        public static ILogger Logger { get; set; }
        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder
                                                        .AddDebug()
                                                        .AddConsole(conf=>conf.IncludeScopes =true)
                                                        .AddApm(config => config.MinimumLogLevel = LogLevel.Information));
            Logger = loggerFactory.CreateLogger(nameof(ILoggerInterceptor));
            //Logger = new Interceptor(Logger);
            Test2();
            Thread.Sleep(15000);
        }

        private static void Test3()
        {
            using (Logger.StartTransactionScope("Test3", "extension"))
            {
                Logger.LogInformation("Log 1");
                using (Logger.BeginScope("Test3","normal"))
                {
                    Logger.LogInformation("Log 2");
                    Logger.AddMetaDataToCurrentTransaction("key1","value1");
                }
            }
        }

        private static void Test2()
        {
            var thread = new Thread(new ThreadStart(TransactionTest));
            thread.Start();
            using (Logger.BeginScope("{apmTransactionName}", "TransactionTest2"))
            {
                Logger.LogInformation("{Apmcustom}", 2);
                using (Logger.BeginScope("{apmSpanName}", "SpanTest2"))
                {
                    var spanThread = new Thread(new ThreadStart(SpanTest));
                    spanThread.Start();
                    Logger.LogInformation("{Apmcustom}", 4);
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
                Logger.LogInformation("{Apmcustom}",1);
                Thread.Sleep(5000);
            }
        }
        static void SpanTest()
        {
            using (Logger.BeginScope("{apmSpanName}", "SpanTest1"))
            {
                Logger.LogInformation("{Apmcustom}", 3);
                Thread.Sleep(5000);
            }
        }
    }
}
