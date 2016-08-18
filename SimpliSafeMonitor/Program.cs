using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Foundation;
using System.ServiceProcess;

namespace SimpliSafeMonitor
{
    static class Program
    {
        static void Main(string[] args)
        {
            bool consoleMode = GeneralUtil.IsVisualStudioMode || Statics.ConsoleMode;
            if (consoleMode)
            {
                Log("Starting in Console Mode");

                new MainService().OnStartExternal();
            }
            else
            {
                Log("Starting in Service Mode");
                ServiceBase.Run(new MainService());
            }
        }

        static void Log(string text) { Log(text, null); }
        static void Log(string text, Exception ex)
        {
            Statics.GetLogger().Log(text, ex);
        }
    }
}
