using System;
using System.Collections.Generic;
using System.Text;

namespace CentralClient
{
    public partial class CentralApi : IDisposable
    {
        bool _isDisposed = false;
        public void Dispose()
        {
            try
            {
                if(!_isDisposed)
                {
                    this._httpClient.Dispose();
                }
            }
            catch { }
            finally { _isDisposed = true; }
        }
    }
}
