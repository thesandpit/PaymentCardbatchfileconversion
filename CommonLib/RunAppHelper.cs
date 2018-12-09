using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class RunAppHelper
    {

        #region Config Options
        private string _doc1GenApp = @"C:\Program Files (x86)\PBBI CCM\DOC1\Generate\doc1gen.exe";
        private string _doc1GenSuccessCode = "GEN0000I";
        #endregion

   

        public bool Doc1Gen(string hipFile, string opsFile, string logFile)
        {
            // run Doc1Gen twice
            // reason: not able to generate the correct barcode for PDF file at the first time
            string doc1CombArguments = string.Format(@"{0} OPS={1}", hipFile, opsFile);
            if (!RunProcess(_doc1GenApp, doc1CombArguments, logFile))
                return false;
            if (!RunProcess(_doc1GenApp, doc1CombArguments, logFile))
                return false;

            return true;
        }


        public bool RunProcess(string appName, string arguments, string logFile)
        {

            int timeout = 60000; //1 min
            var process = new Process();
            bool isSuccess = false;

            try
            {
                process.StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    FileName = appName,
                    Arguments = arguments,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                };
                process.Start();
                string output = DateTime.Now.ToString() + Environment.NewLine + process.StandardOutput.ReadToEnd();
                process.WaitForExit(timeout);

                // Check if get the expected result
                if (IsProcessSuccess(appName, output, logFile))
                {
                    isSuccess = true;
                }
                else
                {
                    Log.Error(string.Format("Fail to run {0} . ExitCode: {1}", appName, process.ExitCode));
                    isSuccess = false;
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
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

        public bool IsProcessSuccess(string fileName, string output, string logFile)
        {
            bool isSuccess = false;

            if (fileName == _doc1GenApp)
            {
                //write process result to log file if required
                if(logFile != null)
                    File.AppendAllText(logFile, output);
                if (output.Contains(_doc1GenSuccessCode))
                    isSuccess = true;
            }
            return isSuccess;
        }
    }
}
