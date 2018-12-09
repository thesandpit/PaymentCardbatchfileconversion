using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class SimplyDataConnections
    {
        public String ConnectionStringIntranet { set; get; }
        public String ConnectionStringSE { set; get; }
        public SimplyDataConnections()
        {
          
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["IsProduction"])) // Check if Production
                {
                    ConnectionStringIntranet = ConfigurationManager.ConnectionStrings["Intranet"].ConnectionString;
                    ConnectionStringSE = ConfigurationManager.ConnectionStrings["SimplyEnergy"].ConnectionString;
                }
                else
                {
                    ConnectionStringIntranet = ConfigurationManager.ConnectionStrings["Intranet_Test"].ConnectionString;
                    ConnectionStringSE = ConfigurationManager.ConnectionStrings["SimplyEnergy"].ConnectionString;
                }
         
        }

    }
}
