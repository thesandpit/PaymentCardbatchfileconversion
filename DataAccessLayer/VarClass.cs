using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class VarClass
    {
        public enum JobType
        {
            PAYMENT_CARDS,
            ENTERTAINMENT_PACKS,
            STELLAR_PREPROCESS,
            WP_DAILY,
            WP_WEEKLY_HUBNET,
            WP_CONFIRMATION,
            WP_WEEKLY_HUBNETNEW,
            STELLAR_INGESTION,
            STELLAR_ENRICHMENT,
            APDM_DISTRUBUTION,
            BILLCAP_ARCHIVING,
            METER_DATA
        }

        public enum TaskTime
        {
            AM,
            PM
        }


        public enum FtpMethod
        {
            LIST,
            DOWNLOAD,
            UPLOAD,
            DELETE
        }

        public enum FtpServer
        {
            ZIPFORM,
            SIMPLY,
            PLACARD
        }

        public enum FileType
        {
            INPUT,
            REPORT
        }

        /// variables change based on JobType
        public static string zipformDir { get; set; }
        public static string downloadPath1 { get; set; }
        public static string downloadPath2 { get; set; }
        public static string batchFile { get; set; }
        public static string zipFile { get; set; }
        public static string zipFileName { get; set; }
        public static string logFilePrefix { get; set; }
        public static string logFile { get; set; }
        public static string logFileName { get; set; }
        public static string doctypeDescription { get; set; }
        public static string backupPath { get; set; }
        public static int dpArchSets { get; set; }
        public static string reportName { get; set; }
        public static string reportStatus { get; set; }
        public static string jobTypeDesc { get; set; }
       

    }
}
