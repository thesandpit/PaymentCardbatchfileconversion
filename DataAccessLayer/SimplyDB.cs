using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;


namespace DataAccessLayer
{
    public class SimplyDB : SimplyDataConnections
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public int GetDocTypeId(string docTypeDescription)
        {
            try
            {
                using (SimplyEnergyDataContext db = new SimplyEnergyDataContext(ConnectionStringSE))
                {
                    var query = db.web_DocTypes.Where(s => s.DocTypeDescription.Equals(docTypeDescription));
                    if (query.Count() > 0)
                        return query.SingleOrDefault().DocTypeId;
                    else
                    {
                        logger.Error("Could not find DocTypeId for " + docTypeDescription);
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return 0;
            }
        }


       
    }
}
