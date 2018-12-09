using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NLog;


namespace DataAccessLayer
{
   public class PaymentCardDB : SimplyDataConnections
    {
       public static Logger logger = LogManager.GetCurrentClassLogger();

       #region Recon Data Received
       public bool InsertDRData(VarClass.JobType jobType)
       {
           try
           {
               using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
               {
                   int DocType = db.Recon_DocTypes.SingleOrDefault(t => t.DT_DocType.Equals(VarClass.doctypeDescription)).DT_UID;

                   var files = jobType.Equals(VarClass.JobType.PAYMENT_CARDS) ? Directory.GetFiles(VarClass.downloadPath1, "*.CSV") : Directory.GetFiles(VarClass.downloadPath1, "*.txt");
                   foreach (string file in files)
                   {
                       Recon_DataReceived data = new Recon_DataReceived
                       {
                           DR_ClientID = 1,
                           DR_JobID = jobType.Equals(VarClass.JobType.PAYMENT_CARDS) ? Int32.Parse(getInputString(0, file)) : Int32.Parse(DateTime.Now.ToString("yyyyMMdd")),
                           DR_Datetime = DateTime.Now,
                           DR_TotalReceived = jobType.Equals(VarClass.JobType.PAYMENT_CARDS) ? Int32.Parse(getInputString(3, file)) : GetEntPackInfo().Item1,
                           DR_DocType = DocType,
                           DR_FileName = jobType.Equals(VarClass.JobType.PAYMENT_CARDS) ? getInputString(2, file) : GetEntPackInfo().Item2,
                       };
                       db.Recon_DataReceiveds.InsertOnSubmit(data);
                       db.SubmitChanges();
                   }
                   return true;
               }
           }
           catch (Exception ex)
           {
               logger.Info(string.Format("InsertDRData Failed. {0}", ex.Message.ToString()));
               return false;
           }

       }


       public bool UpdateDRData(int dpUID)
       {
           try
           {
               using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
               {
                   int DocType = db.Recon_DocTypes.SingleOrDefault(t => t.DT_DocType.Equals(VarClass.doctypeDescription)).DT_UID;
                   var query = from dr in db.Recon_DataReceiveds
                               where dr.DR_DocType == DocType && dr.DR_Datetime.Value.Date == DateTime.Now.Date && dr.DR_DPUID == null
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
               logger.Info(string.Format("UpdateDRData Failed. {0}", ex.Message.ToString()));
               return false;
           }
           return true;
       }
       #endregion


       #region Recon Data Processed
       public bool InsertDPData()
       {
           bool isSuccess = true;
           try
           {
               string[] lines = File.ReadAllLines(VarClass.logFile);
               var targetLine = lines[2].ToString();
               var archSets = targetLine.Split(new[] { "PAGES" }, StringSplitOptions.None)[0].Split('=')[2].Trim();

               using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
               {
                   int DocType = db.Recon_DocTypes.SingleOrDefault(t => t.DT_DocType.Equals(VarClass.doctypeDescription)).DT_UID;
                   int? Division = db.Recon_Divisions.SingleOrDefault(c => c.D_Comment.Equals(VarClass.doctypeDescription)).D_Division;
                   // Query Recon_DataProcessed to get DR_UID based on DocType and DateTime
                   var drUIDs = (from dr in db.Recon_DataReceiveds
                                 where dr.DR_DocType == DocType && dr.DR_Datetime.Value.Date == DateTime.Now.Date
                                 select dr.DR_UID).ToArray();



                   // Insert query results and log file data to Recon_DataProcessed
                   if (drUIDs == null || drUIDs.Length == 0)
                   {
                      logger.Info("Failed to Insert DPData. Could not find related DR_IDs in Recon_DataReceived.");
                   }

                   Recon_DataProcessed data = new Recon_DataProcessed
                    {
                        DP_FileName = VarClass.logFileName,
                        DP_DateProcessed = DateTime.Now,
                        DP_TotalProcessed = 0,
                        DP_MailedSets = 0,
                        DP_EmailedSets = 0,
                        DP_MailedPages = 0,
                        DP_MailedImages = 0,
                        DP_ArchSets = Int32.Parse(archSets),
                        DP_Reconcilled = 0,
                        DP_ReturnedSets = 0,
                        DP_MailedRecords = 0,
                        DP_MailedSetsC4 = 0,
                        DP_Division = Division,
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
                   VarClass.dpArchSets = Int32.Parse(archSets);

                   // Update Recon_DataReceived with returned DP_UID
                   if (!UpdateDRData(data.DP_UID))
                       isSuccess = false;
               }
           }
           catch (Exception ex)
           {
               logger.Info(string.Format("InsertDPData Failed. {0}", ex.Message.ToString()));
               isSuccess = false;
           }

           return isSuccess;
        }

        /// <summary>
        /// Update DP_DateProcessed with Report Data
        /// </summary>
        /// <returns></returns>
        public bool UpdateDPData()
        {
            var reportStatus = "";
            var reportPaths = Directory.GetFiles(VarClass.downloadPath1, string.Format("{0}*", VarClass.reportName));
            if (reportPaths == null) return false;

            foreach (var reportPath in reportPaths)
            {
                if (!UpdateSingleDPData(reportPath, ref reportStatus))
                    return false;
            }

            //reportStatus format: e.g. 22/10/2014 Description : 98
            VarClass.reportStatus = reportStatus;
            return true;
        }


        /// <summary>
        /// Update DP_DateProcessed one record at a time
        /// </summary>
        /// <param name="reportPath"></param>
        /// <returns></returns>
        public bool UpdateSingleDPData(string reportPath, ref string reportStatus)
        {
            try
            {
                    using (SimplyPaymentCardDataContext db = new SimplyPaymentCardDataContext(ConnectionStringIntranet))
                    {

                        var lines = File.ReadLines(reportPath).ToArray();
                        if (lines == null)
                        {
                            logger.Info("Fail to Update DP_DateProcessed. No record in Placard Report.");
                        }

                        for (int i = 1; i < lines.Count(); i++)
                        {
                            if (lines[i].Contains(","))
                            {
                                var lineSplit = lines[i].Split(',');
                                var desc= lineSplit[3];
                                var dateReceived = lineSplit[4];
                                int jobId = Int32.Parse(lineSplit[1].Split('_')[0]);
                                int subtotal = Int32.Parse(lineSplit[7]);

                                // Update DP_TotalProcessed in [DP_DateProcessed] for a specific day
                                string fileName = String.Format("{0}{1}.txt", VarClass.logFilePrefix, Convert.ToDateTime(dateReceived).ToString("yyyyMMdd"));
                                var query = from dp in db.Recon_DataProcesseds
                                            where dp.DP_FileName == fileName && dp.DP_DateProcessed.Value.Date == Convert.ToDateTime(dateReceived).Date
                                            select dp;

                                if (query.Count() > 1)
                                {
                                    logger.Info("Fail to Update DP_DateProcessed. There is more than one record exist");
                                }

                                if (query.Count() < 1)
                                {
                                    logger.Info("Fail to Update DP_DateProcessed. There is no record exist");
                                }

                            
                                // DP_TotalProcessed = initial DP_TotalProcessed value + total processed in the report
                                int iniTotalProcessed = query.SingleOrDefault().DP_TotalProcessed.Value;
                                int totalProcessed = iniTotalProcessed + subtotal;
                                query.SingleOrDefault().DP_TotalProcessed = totalProcessed;
                                db.SubmitChanges();
                                reportStatus += string.Format("{0} {1}: {2} {3}", dateReceived, desc, subtotal, Environment.NewLine);
                            }

                        }
        
                        return true;
                        
                    }
            }
            catch (Exception ex)
            {
                logger.Info(string.Format("Fail to Update DP_DateProcessed. {0}", ex.Message.ToString()));
                return false;
            }
        }
       #endregion


       #region Private Methods
        private string getInputString(int lineNo, string file)
        {
            //Get and read csv file       
            var lines = File.ReadLines(file).Take(4).ToArray();
            int titleLength = 28;
            int datalength = lines[lineNo].Length - titleLength + 1;
            return lines[lineNo].Substring(titleLength - 1, datalength).Trim();
        }

        private Tuple<int, string> GetEntPackInfo()
        {
            string file = Directory.GetFiles(VarClass.downloadPath1, "*.txt").SingleOrDefault();
            FileInfo fileInfo = new FileInfo(file);
            var fileName = fileInfo.Name;


            int counter = 0;
            using (StreamReader r = new StreamReader(file))
            {
                while (r.ReadLine() != null) { counter++; }
            }
            //the first line is title
            return new Tuple<int, string>(counter - 1, fileName);
        }
        #endregion
    }
}
