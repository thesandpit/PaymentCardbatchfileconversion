using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.Linq;
using System.IO.Compression;

using DataAccessLayer;
using Microsoft.SqlServer.Server;

namespace CommonLib
{
    public class ArchiveHelper
    {
        private readonly static string connIntraTestString = ConfigurationManager.AppSettings["Intranet_Test"]; 
        private static HelperClass objHelper = new HelperClass();

        /// <summary>
        /// Looks up Simply Energy jobNo from database and archives file into the "\Data\" directory on the server
        /// </summary>
        /// <param name="fileToArch">Path to the file to archive</param>
        /// <param name="deleteAfterArch">Option to delete the original source file after archiving</param>
        /// <param name="appendDateStamp">Appends "ddMMyyyy" stamp onto the archive file name</param>
        /// <returns></returns>
        public bool ArchiveToJobFolder(string fileToArch, bool deleteAfterArch, bool appendDateStamp)
        {
            bool isSuccessful = true;
            DBHelper objDalRecon = new DBHelper();
            int clientId = objHelper.parseInt32(ConfigurationManager.AppSettings["SimplyClientID"]);
            int divisonId = objHelper.parseInt32(ConfigurationManager.AppSettings["SimplyDivisionID"]);
            string jobArchBaseDir = ConfigurationManager.AppSettings["JobsBaseFolder"];

            DateTime monthFormat = FrstDayOfMnthFrmDateTime().GetValueOrDefault();
            string reconRcrdJobNo = objDalRecon.GetJobNumber(monthFormat, divisonId, clientId);
            if (reconRcrdJobNo != null)
            {
                string archJobBaseFolder = reconRcrdJobNo.Substring(0, 3) + "00";
                string destinationArchFolder = Path.Combine(jobArchBaseDir, archJobBaseFolder);  // i.e. \\ServerName\jobs\54300

                var arrDirs = Directory.GetDirectories(destinationArchFolder);

                // Get sub directory matching jobNo
                string fullPath = null;
                if (arrDirs == null) return false;
                foreach (var directory in arrDirs)
                {
                    string tempPath = directory.Replace(destinationArchFolder, "").Replace(@"\","");
                    tempPath = tempPath.Substring(0, 5);
                    if (tempPath == reconRcrdJobNo)
                        fullPath = directory;
                }

                if (fullPath == null) return false;
                
                //Exists: navigate to /data/ dir
                string strDataDir = Path.Combine(fullPath, "Data");
                try
                {
                    if (Directory.Exists(strDataDir))
                    {
                        string tempPath = Path.Combine(Path.GetDirectoryName(fileToArch),"zipTemp");
                        if (!Directory.Exists(tempPath))
                            Directory.CreateDirectory(tempPath);
                         
                        string tempFilePath = Path.Combine(tempPath, Path.GetFileName(fileToArch));
                        File.Copy(fileToArch,tempFilePath,true);
                            
                        string fileToArchDir = Path.GetFileName(fileToArch).Replace(Path.GetExtension(fileToArch), "");
                        if (appendDateStamp)
                            fileToArchDir = String.Format("{0}_{1}", fileToArchDir, DateTime.Now.ToString("ddMMyyyy"));

                        string archFilePath = Path.Combine(strDataDir, fileToArchDir);                     // Archive location
                        archFilePath = archFilePath + ".zip";
                        
                        // Zip if it doesn't already exist
                        if (!File.Exists(archFilePath))
                            ZipFile.CreateFromDirectory(tempPath, archFilePath);
                        else
                        {
                            File.Delete(archFilePath);
                            ZipFile.CreateFromDirectory(tempPath, archFilePath);
                        }
                            
                        // Cleanup
                        File.Delete(tempFilePath);
                        Directory.Delete(tempPath);
                            
                        // Remove original file
                        if(deleteAfterArch)
                            File.Delete(fileToArch);
                    }
                          
                }
                catch (Exception ex)
                {
                    isSuccessful = false;
                }
                
            }
            
            return isSuccessful;
        }


        /// <summary>
        /// Looks up Simply Energy jobNo from database and archives file into the "\Data\" directory on the server
        /// </summary>
        /// <param name="fileToArch">Path to the file to archive</param>
        /// <param name="deleteAfterArch">Option to delete the original source file after archiving</param>
        /// <param name="appendDateStamp">Appends "ddMMyyyy" stamp onto the archive file name</param>
        /// <param name="clientId">ClientId field reference in database</param>
        /// <param name="divisionId">DivisionId field reference in database</param>
        /// <returns></returns>
        public bool ArchiveToJobFolder(string fileToArch, bool deleteAfterArch, bool appendDateStamp, int clientId, int divisionId)
        {
            bool isSuccessful = true;
            DBHelper objDalRecon = new DBHelper();
            string jobArchBaseDir = ConfigurationManager.AppSettings["JobsBaseFolder"];

            DateTime monthFormat = FrstDayOfMnthFrmDateTime().GetValueOrDefault();
            string reconRcrdJobNo = objDalRecon.GetJobNumber(monthFormat, divisionId, clientId);
            if (reconRcrdJobNo != null)
            {
                string archJobBaseFolder = reconRcrdJobNo.Substring(0, 3) + "00";
                string destinationArchFolder = Path.Combine(jobArchBaseDir, archJobBaseFolder);  // i.e. \\ServerName\jobs\54300

                var arrDirs = Directory.GetDirectories(destinationArchFolder);

                // Get sub directory matching jobNo
                string fullPath = null;
                if (arrDirs == null) return false;
                foreach (var directory in arrDirs)
                {
                    string tempPath = directory.Replace(destinationArchFolder, "").Replace(@"\", "");
                    tempPath = tempPath.Substring(0, 5);
                    if (tempPath == reconRcrdJobNo)
                        fullPath = directory;
                }

                if (fullPath == null) return false;

                //Exists: navigate to /data/ dir
                string strDataDir = Path.Combine(fullPath, "Data");
                try
                {
                    if (Directory.Exists(strDataDir))
                    {
                        string tempPath = Path.Combine(Path.GetDirectoryName(fileToArch), "zipTemp");
                        if (!Directory.Exists(tempPath))
                            Directory.CreateDirectory(tempPath);

                        string tempFilePath = Path.Combine(tempPath, Path.GetFileName(fileToArch));
                        File.Copy(fileToArch, tempFilePath, true);

                        string fileToArchDir = Path.GetFileName(fileToArch).Replace(Path.GetExtension(fileToArch), "");
                        if (appendDateStamp)
                            fileToArchDir = String.Format("{0}_{1}", fileToArchDir, DateTime.Now.ToString("ddMMyyyy"));

                        string archFilePath = Path.Combine(strDataDir, fileToArchDir);                     // Archive location
                        archFilePath = archFilePath + ".zip";

                        // Zip if it doesn't already exist
                        if (!File.Exists(archFilePath))
                            ZipFile.CreateFromDirectory(tempPath, archFilePath);
                        else
                        {
                            File.Delete(archFilePath);
                            ZipFile.CreateFromDirectory(tempPath, archFilePath);
                        }
                        // Cleanup
                        File.Delete(tempFilePath);
                        Directory.Delete(tempPath);

                        // Remove original file
                        if (deleteAfterArch)
                            File.Delete(fileToArch);
                    }

                }
                catch (Exception ex)
                {
                    isSuccessful = false;
                }

            }

            return isSuccessful;
        }

        private static DateTime? FrstDayOfMnthFrmDateTime()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
        }

    }

        
}
