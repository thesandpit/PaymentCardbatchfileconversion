using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using DataAccessLayer;
using System.Configuration;

namespace CommonLib
{
    public class Log
    {
        public enum LogLevels { TRACE, DEBUG, WARN, INFO, ERROR, FATAL };
        public static Logger logger = LogManager.GetCurrentClassLogger();


        public static void Trace(string newEntry)
        {
            CreateLog(newEntry, LogLevels.TRACE);
        }

        public static void Info(string msg)
        {
            var newEntry = VarClass.jobTypeDesc + "|" + msg;
            CreateLog(newEntry, LogLevels.INFO);
        }


        public static void Error(string msg)
        {
            var newEntry = VarClass.jobTypeDesc + "|" + msg;
            CreateLog(newEntry, LogLevels.ERROR);
        }

        public static void Error(Exception ex)
        {
            CreateLog(ex.ToString(), LogLevels.ERROR);
        }


        public static void Fatal(string newEntry)
        {
            //var emailBody = BuildErrorEmail(newEntry);
            GlobalDiagnosticsContext.Set("eSubject", "Simply Windows Service Process Error");
            GlobalDiagnosticsContext.Set("eBody", BuildErrorEmail(newEntry));
            GlobalDiagnosticsContext.Set("eFrom", ConfigurationManager.AppSettings["DefaultErrorMailFrom"]);
            GlobalDiagnosticsContext.Set("eTo", ConfigurationManager.AppSettings["DefaultMailRecipient3"]);
            CreateLog(newEntry, LogLevels.FATAL);
        }


        private static void CreateLog(string newEntry, LogLevels level)
        {
            try
            {

                switch (level)
                {
                    case LogLevels.TRACE:
                        logger.Trace(newEntry);
                        break;
                    case LogLevels.DEBUG:
                        logger.Debug(newEntry);
                        break;
                    case LogLevels.WARN:
                        logger.Warn(newEntry);
                        break;
                    case LogLevels.INFO:
                        logger.Info(newEntry);
                        break;
                    case LogLevels.ERROR:
                        logger.Error(newEntry);
                        break;
                    case LogLevels.FATAL:
                        logger.Fatal(newEntry);
                        break;
                    default:
                        logger.Info(newEntry);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private static string BuildErrorEmail(string msg)
        {

            var logFile = @"\\doc2\Prod\Simply_Energy_Refactor\Application\bin\logs";
            return String.Format("Simply Windows Service Error:\n\r <b>{0}</b>\n\r\n\rPlease refer to log file for details:\n\r<a href={1}>{1}</a>\n\r\n\rGood Luck!", msg, logFile);
        }

    }
}
