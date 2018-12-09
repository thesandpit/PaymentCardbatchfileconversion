using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class DBHelper: SimplyDataConnections
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        #region Recon_JobNumbers
        public string GetJobNumber(DateTime month, int divisionId, int clientId)
        {
            string strJobNo = null;
            try
            {
                using (SimplyPaymentCardDataContext objDb = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    Recon_JobNumber objRecnJN = (from jobNo in objDb.Recon_JobNumbers where jobNo.JN_Month == month && jobNo.JU_ClientID == clientId && jobNo.JU_Division == divisionId select jobNo).SingleOrDefault();
                    if (objRecnJN != null)
                    {
                        strJobNo = objRecnJN.JN_JobNumber.ToString();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return strJobNo;
        }
        #endregion

        #region Recon_DocType
        public int GetDocTypeByDescription(string description)
        {
            bool isSuccess = false;
            Recon_DocType query = new Recon_DocType();

       
                
            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    query = db.Recon_DocTypes.Where(t => t.DT_DocType.Equals(description)).SingleOrDefault();
                    if (query != null) isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                logger.Info(MethodBase.GetCurrentMethod().Name + ex.Message);
            }

            if (isSuccess)
                return query.DT_UID;
            else
                return -1;
        }

        #endregion


        #region Recon_DataReceived
        public bool InsertReconDataReceived(Recon_DataReceived input)
        {
            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    //Check if the FileName already exists
                    var fileNameQuery = db.Recon_DataReceiveds.Where(f => f.DR_FileName.Equals(input.DR_FileName));
                    if (fileNameQuery.Count() >= 1)
                    {
                        logger.Info(MethodBase.GetCurrentMethod().Name + "DR_FileName already exists");
                        return false;
                    }

                    db.Recon_DataReceiveds.InsertOnSubmit(input);
                    db.SubmitChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Info(MethodBase.GetCurrentMethod().Name + ex.Message);
                return false;
            }

        }


        public int GetDRUIDByFileName(string fileName)
        {
            bool isSuccess = false;
            Recon_DataReceived query = new Recon_DataReceived();

            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    query = db.Recon_DataReceiveds.Where(vrr => vrr.DR_FileName.Equals(fileName)).SingleOrDefault();
                    if (query != null) isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                logger.Info(MethodBase.GetCurrentMethod().Name + ex.Message);
            }

            if (isSuccess)
                return query.DR_UID;
            else
                return -1;

        }

        //public bool InsertReconDataReceived(DBModels.ReconDataReceivedModel model)
        //{
        //    try
        //    {
        //        using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionString))
        //        {
        //            //Check if the FileName already exists
        //            var fileNameQuery = db.Recon_DataReceiveds.Where(f => f.DR_FileName.Equals(model.DR_FileName));
        //            if (fileNameQuery.Count() >= 1)
        //            {
        //                logger.Info(MethodBase.GetCurrentMethod().Name + "DR_FileName already exists");
        //                return false;
        //            }

        //            var query = db.Recon_DocTypes.SingleOrDefault(t => t.DT_DocType.Equals(model.DocTypeDesc));
        //            if (query == null) return false;
        //            int DocType = query.DT_UID;

                  
        //            Recon_DataReceived data = new Recon_DataReceived
        //            {
        //                DR_ClientID = model.DR_ClientID,
        //                DR_JobID = model.DR_JobID,
        //                DR_Datetime = DateTime.Now,
        //                DR_TotalReceived = model.DR_TotalReceived,
        //                DR_DocType = DocType,
        //                DR_FileName = model.DR_FileName,
        //            };
        //            db.Recon_DataReceiveds.InsertOnSubmit(data);
        //            db.SubmitChanges();
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Info(string.Format("Fail to Insert data to ReconDataReceived. {0}", ex.Message.ToString()));
        //        return false;
        //    }

        //}

        public bool UpdateReconDataReceived(int dpUID, int docType)
        {
            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    var query = from dr in db.Recon_DataReceiveds
                                where dr.DR_DocType == docType && dr.DR_Datetime.Value.Date == DateTime.Now.Date && dr.DR_DPUID == null    
                                select dr;

                    foreach (Recon_DataReceived dr in query)
                    {
                        dr.DR_DPUID = dpUID;
                    }
                    db.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Info(string.Format("Fail to Update ReconDataReceived. {0}", ex.Message.ToString()));
                return false;
            }
            return true;
        }
        # endregion



        #region Recon_DataProcessed
        public bool InsertReconDataProcessed(DBModels.ReconDataProcessedModel input)
        {
            bool isSuccess = true;

            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {


                    int docType = GetDocTypeByDescription(input.DocTypeDesc);
                    if(docType == -1) return false;
                    int? division = db.Recon_Divisions.SingleOrDefault(c => c.D_Comment.Equals(input.DocTypeDesc)).D_Division;
                    // Query Recon_DataReceiveds to get DR_UID based on DocType and DateTime
                    var drUIDs = (from dr in db.Recon_DataReceiveds
                                  where dr.DR_DocType == docType && dr.DR_Datetime.Value.Date == DateTime.Now.Date
                                  select dr.DR_UID).ToArray();

                    // Insert data to Recon_DataProcessed
                    if (drUIDs == null)
                    {
                        logger.Info("Fail to Insert ReconDataProcessed. Could not find related DR_IDs in Recon_DataReceived.");
                    }

                    Recon_DataProcessed data = new Recon_DataProcessed
                    {
                        DP_FileName = input.DP_FileName,
                        DP_DateProcessed = DateTime.Now,
                        DP_TotalProcessed = input.DP_TotalProcessed,
                        DP_MailedSets = 0,
                        DP_EmailedSets = 0,
                        DP_MailedPages = 0,
                        DP_MailedImages = 0,
                        DP_ArchSets = 0,
                        DP_Reconcilled = 0,
                        DP_ReturnedSets = 0,
                        DP_MailedRecords = 0,
                        DP_MailedSetsC4 = 0,
                        DP_Division = division,
                        DR_IDs = string.Join(",", drUIDs),      //TODO: Not testing for null on linq select
                        DP_BUNCarryOverRecords = 0,
                        DP_EmailPages = 0,
                        DP_EmailedImages = 0,
                        DP_EmailedSets2 = 0,
                        DP_Ignored = 0,
                        DP_ArchiveOnly = 0,
                        DP_EmailedRecords = 0,
                        DP_ReturnedRecords = 0,
                        DP_PrioritySets = 0,
                    };
                    db.Recon_DataProcesseds.InsertOnSubmit(data);
                    db.SubmitChanges();

                    // Update Recon_DataReceived with returned DP_UID
                    if (!UpdateReconDataReceived(data.DP_UID, docType))
                        isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                logger.Info(string.Format("Fail to Insert ReconDataProcessed. {0}", ex.Message.ToString()));
                isSuccess = false;
            }

            return isSuccess;
        }
        # endregion

        #region Recon_Trackings
        public bool InsertReconTracking(Recon_Tracking input)
        {
            try
            {
                using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                {
                    db.Recon_Trackings.InsertOnSubmit(input);
                    db.SubmitChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Info(string.Format("Fail to Insert ReconTrackings. {0}", ex.Message.ToString()));
                return false;
            }

        }


  
        # endregion



    }
}
