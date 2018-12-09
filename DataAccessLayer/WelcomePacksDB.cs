using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    
    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }

    public class WelcomePacksDB : SimplyDataConnections
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();


        public Recon_Tracking GetReconTracking(string fileName, ref bool expired)
        {
            try
            {
                var result = new Recon_Tracking();
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    result = db.Recon_Trackings.Where(s => s.FileName.Equals(fileName)).OrderByDescending(o => o.DR_UID).FirstOrDefault();
                    if (result == null) return null;

                    var dataReceived = db.Recon_DataReceiveds.Where(dr => dr.DR_UID == result.DR_UID).SingleOrDefault();
                    if (dataReceived == null)
                    {
                        logger.Info(string.Format("Could not find record in [Recon_DataReceiveds]. DR_UID: {0}, Tracking Number: {1}", result.DR_UID, result.TrackingNo));
                        return null;
                    }
                      
                    DateTime expiryDate = dataReceived.DR_Datetime.Value.AddMonths(3);
                    if (expiryDate < DateTime.Now)
                    {
                        expired = true;
                    }
                }

                return result;

            }
            catch (Exception ex)
            {
                logger.Info(string.Format("FileName:{0}. Error:{1}",fileName , ex.Message));
                return null;

            }
        }

        public bool UpdateReconTracking(Recon_Tracking model)
        {
            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    var trackingData = db.Recon_Trackings.Where(t => t.TrackingNo == model.TrackingNo);         
                    if (trackingData.Count() > 0)
                    {
                        foreach (var record in trackingData)
                        {
                            record.AddressLine1 = model.AddressLine1.Truncate(60);
                            record.AddressLine2 = model.AddressLine2.Truncate(60);
                            record.AddressLine3 = model.AddressLine3.Truncate(60);
                            record.Subub = model.Subub.Truncate(30); 
                            record.State = model.State.Truncate(3);
                            record.Postcode = model.Postcode.Truncate(4);
                            record.FileName = model.FileName;           
    
                        }

                        db.SubmitChanges();
        
                    }


                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Info("failed to update tracking data. Tracking number:" + model.TrackingNo  + ex.Message);
                return false;
            }
        }

        public bool UpdateReconTracking2(Recon_Tracking model)
        {
            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    var trackingData = db.Recon_Trackings.Where(t => t.TrackingNo == model.TrackingNo);
                    if (trackingData.Count() > 1)
                    {

                        var data = trackingData.Where(s => s.FileName == model.FileName);
                        if (data.Count() == 0)
                        {
                            var query = trackingData.Where(s => s.Flag == false);
                            if(query.Count() > 0)
                                query.First().FileName = model.FileName;
                            else
                                trackingData.First().FileName = model.FileName;


                            db.SubmitChanges();
                        }

                    }


                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Info("failed to update tracking data. Tracking number:" + model.TrackingNo + ex.Message);
                return false;
            }
        }


        public bool UpdateReconTracking(Recon_Tracking model, ref HashSet<int> druidUpdateList)
        {
            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    var trackingData = db.Recon_Trackings.Where(t => t.TrackingNo == model.TrackingNo);
                    if (trackingData.Count() > 0)
                    {
                        foreach (var record in trackingData)
                        {
                            //check if the record is received within 3 month
                            //If within 3 months, update the Recon_Tracking and Recon_DataProcessed table
                            int druid = record.DR_UID;
                            var dataReceived = db.Recon_DataReceiveds.Where(dr => dr.DR_UID == druid).SingleOrDefault();
                            if (dataReceived == null) return false;
                            DateTime expiryDate = dataReceived.DR_Datetime.Value.AddMonths(3);
                            if (expiryDate > DateTime.Now)
                            {
                                record.Flag = model.Flag;
                            }

                            druidUpdateList.Add(record.DR_UID);
                        }

                        db.SubmitChanges();

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Info(MethodBase.GetCurrentMethod().Name + ex.Message);
                return false;
            }
        }


        public bool UpdateReconDP(ref HashSet<int> druidUpdateList)
        {
            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {

                    foreach (var druid in druidUpdateList)
                    {
                        // Update data in Recon_DataProcessed 
                        var dataProcessed = db.Recon_DataProcesseds.Where(dp => dp.DR_IDs == druid.ToString()).SingleOrDefault();
                        if (dataProcessed == null)
                        {
                            logger.Info("DR_ID:" + druid + " not exist in Recon_DataProcessed");
                            return false;
                        }
                        dataProcessed.DP_ArchSets = db.Recon_Trackings.Where(t => t.DR_UID == druid && t.Flag == true).Count();
                    }

                    db.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Info(MethodBase.GetCurrentMethod().Name + ex.Message);
                return false;
            }
        }

        public bool UpdateReconWithConfirmCSVOld(Recon_Tracking model)
        {
            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    var trackingData =  db.Recon_Trackings.Where(t => t.TrackingNo == model.TrackingNo);
                    if (trackingData.Count() == 0)
                    {
                        logger.Info("Tracking No " + model.TrackingNo + " not exist in Recon_Tracking");
                         return false;
                    }
                    if (trackingData.Count() > 1)
                    {
                        logger.Info("Tracking No " + model.TrackingNo + " has more than 1 record in Recon_Tracking");
                        return false;
                    }

                    
                    //check if the record is received within 3 month
                    //If within 3 months, update the Recon_Tracking and Recon_DataProcessed table
                    int druid = trackingData.SingleOrDefault().DR_UID;
                    var dataReceived = db.Recon_DataReceiveds.Where(dr => dr.DR_UID == druid).SingleOrDefault();
                    if (dataReceived == null) return false;
                    DateTime expiryDate = dataReceived.DR_Datetime.Value.AddMonths(3);

                    if (expiryDate > DateTime.Now)
                    {
                        // Update data in Recon_Tracking  
                        trackingData.SingleOrDefault().Flag = model.Flag;

                        // Update data in Recon_DataProcessed 
                        int dpuid = dataReceived.DR_DPUID.Value;
                        var dataProcessed = db.Recon_DataProcesseds.Where(dp => dp.DP_UID == dpuid).SingleOrDefault();
                        if (dataProcessed == null)
                        {
                            logger.Info("Tracking Record " + model.TrackingNo + " not exist in Recon_DataProcessed");
                            return false;
                        }
                        //If the result for the tracking number is true, then increase DP_ArchSets by 1
                        if (model.Flag == true)
                        {
                            dataProcessed.DP_ArchSets = dataProcessed.DP_ArchSets + 1;
                        }

                        db.SubmitChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Info(MethodBase.GetCurrentMethod().Name + ex.Message);
                return false;
            }
        }
    }
}
