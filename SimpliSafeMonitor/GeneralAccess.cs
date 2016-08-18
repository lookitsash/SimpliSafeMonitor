using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using System.Data.SqlClient;
using System.Data;


namespace SimpliSafeMonitor
{
    public class GeneralAccess : SmartSql
    {
        public GeneralAccess(string connectionString) : base(connectionString) { }
    }
}
