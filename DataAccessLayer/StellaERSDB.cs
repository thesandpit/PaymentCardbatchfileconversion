using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class StellaERSDB : SimplyDataConnections
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public bool DeleteAllStellaERSRates()
        {
            try
            {

                using (SimplyEnergyDataContext db = new SimplyEnergyDataContext(ConnectionStringSE))
                {
                    var query = db.stella_ERSRates;
                    db.stella_ERSRates.DeleteAllOnSubmit(query);
                    db.SubmitChanges();
                    if (query.Count() == 0)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                logger.Info(MethodBase.GetCurrentMethod().Name + ":" + ex);
                return false;
            }



        }
        
        
        public bool InsertStellaERSRates(stella_ERSRate input)
        {
            try
            {
                using (SimplyEnergyDataContext db = new SimplyEnergyDataContext(ConnectionStringSE))
                {
                    db.stella_ERSRates.InsertOnSubmit(input);
                    db.SubmitChanges();
                }
                return true;   
            }
            catch (Exception ex)
            {
                logger.Info(MethodBase.GetCurrentMethod().Name +":" + ex.Message);
                return false;
            }
        }


        public bool GetStellaERSRates(string productCode, string rateRequestedDateStr, ref List<string> lstERSData)
        {
            try
            {
                DateTime rateRequestedDate = Convert.ToDateTime(rateRequestedDateStr); 
                using (SimplyEnergyDataContext db = new SimplyEnergyDataContext(ConnectionStringSE))
                {
                    var results = db.stella_ERSRates.Where(s => s.ProductCode.Equals(productCode) && rateRequestedDate >= s.StartDate && rateRequestedDate <= s.EndDate);
                    foreach (var item in results)        
                    {
                        lstERSData.Add(item.DetailsField);
                    }
     
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Info(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                return false;
            }
        }

    }
}
