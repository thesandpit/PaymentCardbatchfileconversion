using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace CommonLib
{
    public class RunProcessHelper
    {
        # region Declare
        //public static string _binPath = ConfigurationManager.AppSettings["StatementsBinPath"];
        //public static string _zfFixXMLApp = Path.Combine(_binPath, ConfigurationManager.AppSettings["ZFFixXMLApp"]);
        //public static string _dfXMLInputFile = Path.Combine(ConfigurationManager.AppSettings["StatementsDataPath"], ConfigurationManager.AppSettings["DFXMLInput"]);
        //public static string _dfApp = ConfigurationManager.AppSettings["DFApp"];
        //public static string _doc1GenApp = ConfigurationManager.AppSettings["Doc1GenApp"];
        //public static string _doc1GenLogFile = Path.Combine(_binPath, ConfigurationManager.AppSettings["Doc1GenLog"]);
        #endregion


        private static bool RunLocalProcess(string fileName, string arguments)
        {
            int timeout = 60000; //1 min
            var process = new Process();
            bool isSuccess = false;

            try
            {
                process.StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    RedirectStandardError = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    FileName = fileName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                };
                process.Start();
                process.WaitForExit(timeout);
                isSuccess = true;
               
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                isSuccess = false;
            }
            finally
            {
                if (process != null)
                {
                    process.Close();
                }
            }
            return isSuccess;
        }


        private static bool RunWcfProcess(string exePath, string parameter)
        {
            bool isSuccessful = true;
            try
            {
                var objClient = new WcfServiceReference.WcfServiceClient();
                objClient.RunApp(exePath, parameter);
            }
            catch (Exception ex)
            {
                isSuccessful = false;
            }

            return isSuccessful;
        }

        public static bool RunProcess(bool isProduction, string exePath, string parameter)
        {
            bool isSuccess = true;

            if (isProduction)
              isSuccess = RunLocalProcess(exePath, parameter);
            else
              isSuccess = RunWcfProcess(exePath, parameter);

            return isSuccess;
        }


        //    public static bool IsProcessSuccess(string fileName, string output)
        //    {
        //        bool isSuccess = false;


        //        if (fileName == _zfFixXMLApp)
        //        {
        //            if (File.Exists(_dfXMLInputFile))
        //                isSuccess = true;
        //        }

        //        if (fileName == _dfApp)
        //        {
        //            if (output.Contains("Completed"))
        //                isSuccess = true;
        //        }

        //        if (fileName == _doc1GenApp)
        //        {
        //            //write process result to log file
        //            File.AppendAllText(_doc1GenLogFile, output);
        //            if (output.Contains("Run completed successfully"))
        //                isSuccess = true;
        //        }
        //        return isSuccess;
        //    }
    }
}
