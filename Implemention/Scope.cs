using Elastic.Apm;
using ILoggerInterceptor.Abstractions;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ILoggerInterceptor.Implemention
{
    class Scope : IScope
    {
        public IDisposable LoggerScope { get; }
        bool disposed = false;
        readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        public Scope(IDisposable loggerScope)
        {
            LoggerScope = loggerScope;
        }

        public Scope()
        {
            LoggerScope = null;
        }



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                LoggerScope?.Dispose();
                if(Agent.Tracer.CurrentSpan == null)
                {
                    Agent.Tracer.CurrentTransaction.End();
                }
                else
                {
                    Agent.Tracer.CurrentSpan.End();
                }
            }

            disposed = true;
        }

        ~Scope()
        {
            Dispose(false);
        }
    }
}
