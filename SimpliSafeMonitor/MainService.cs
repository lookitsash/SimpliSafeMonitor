using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Foundation;

namespace SimpliSafeMonitor
{
    public partial class MainService : ServiceBase
    {
        #region ServiceBase
        public MainService()
        {
            InitializeComponent();
        }

        internal void OnStartExternal()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            new Thread(new ThreadStart(Initialize)) { Name = "Initialize" }.Start();
        }

        protected override void OnStop()
        {
            MasterController.Shutdown();
        }
        #endregion

        void Initialize()
        {
            (Statics.MonitorProcessor = new MonitorProcessor()).Start();
        }

        void MasterController_ShutdownComplete()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Log("Shutdown Exception", ex);
            }
        }

        void Log(string text) { Log(text, null); }
        void Log(string text, Exception ex)
        {
            Statics.GetLogger().Log(text, ex);
        }
    }
}
