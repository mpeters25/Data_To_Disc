using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace bluRaySizer
{
    class SimpleFileInfo : IDisposable
    {
        public string Path { get; set; }
        public double SizeMb { get; set; }

        private bool disposed = false;
        private SafeHandle handle;

        public void Dispose()
        {
            //Dispose of unmanaged resources
            Dispose(true);
            //Suppress finalization
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (handle != null) handle.Dispose();
                }
                disposed = true;
            }
        }

        ~SimpleFileInfo()
        {
            //Log.LogWrite("SimpleFileInfo destructor called.");
        }

    }
}
