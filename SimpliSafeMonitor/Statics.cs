using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;

namespace SimpliSafeMonitor
{
    public class Statics
    {
        static Statics()
        {
            Logger.SetDefaultFilePathFormat(Statics.Log_Directory);
        }

        public static string DbConnectionString { get { return ConfigAdapter.GetConnectionString("DbConnectionString"); } }
        public static GeneralAccess GetGeneralAccess() { return new GeneralAccess(DbConnectionString); }

        public static Logger GetLogger() { return GetLogger(String.Empty); }
        public static Logger GetLogger(string name) { return new Logger(name); }
        public static string Log_Directory { get { return ConfigAdapter.GetAppSetting("Log_Directory"); } }

        public static bool ConsoleMode { get { return ConfigAdapter.GetAppSetting<bool>("ConsoleMode"); } }

        public static MonitorProcessor MonitorProcessor = null;
    }
}
