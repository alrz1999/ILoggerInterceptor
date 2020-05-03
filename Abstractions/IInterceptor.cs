using Elastic.Apm.Api;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ILoggerInterceptor.Abstractions
{
    interface IInterceptor  : ILogger
    {

    }
}
