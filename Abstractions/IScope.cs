using System;
using System.Collections.Generic;
using System.Text;

namespace ILoggerInterceptor.Abstractions
{
    interface IScope : IDisposable
    {
        IDisposable LoggerScope { get; }
    }
}
