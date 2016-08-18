using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;

namespace SimpliSafeMonitor
{
    public class MonitorProcessor : Processor
    {
        protected override void Pulse()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Log("Pulse Exception", ex);
            }
        }
    }
}
