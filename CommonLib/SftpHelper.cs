using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

namespace CommonLib
{
    public class SftpHelper
    {

        private SftpClient objSftpClient;
        private string _currDirectoryLoc;


        #region Init
        /// <summary>
        /// SftpHelper Constructor 
        /// Creating connection info using standard account authentication
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="hostName"></param>
        /// <param name="portNumber">Port 22 is used by default if null</param>
        public SftpHelper(string userName, string password, string hostName, int? portNumber)
        {
            int portNum = portNumber.HasValue ? portNumber.GetValueOrDefault() : 22;        // Set port 22 by default
            try
            {
                
                var connectionInfo = new ConnectionInfo(hostName, portNum, userName, new PasswordAuthenticationMethod(userName, password));
                objSftpClient = new SftpClient(connectionInfo);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Could not initilize Sftp object. Ex: {0}", ex.Message));
            }

        }

         /// <summary>
        /// SftpHelper Constructor 
        /// Creating connection info using standard account authentication
        /// Explitly setting working directory
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="hostName"></param>
        /// <param name="portNumber">Port 22 is used by default if null</param>
        /// <param name="sftpWorkingDir"></param>
        public SftpHelper(string userName, string password, string hostName, int? portNumber, string sftpWorkingDir)
        {
            int portNum = portNumber.HasValue ? portNumber.GetValueOrDefault() : 22;        // Set port 22 by default
             try
             {
                 var connectionInfo = new ConnectionInfo(hostName, portNum, userName, new PasswordAuthenticationMethod(userName, password));
                 objSftpClient = new SftpClient(connectionInfo);
             }
             catch (Exception ex)
             {
                 throw new Exception(String.Format("Could not initilize Sftp object. Ex: {0}", ex.Message));
             }

        }

        /// <summary>
        /// SftpHelper Constructor 
        /// Creating connection info using SSH/SFTP private/public key handshake
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="hostName"></param>
        /// <param name="portNumber"></param>
        /// <param name="privteKyPth"></param>
        /// <param name="privteKyPassPhrase">The private key pharse used to open/access private key file</param>
        public SftpHelper(string userName, string password, string hostName, int? portNumber, string privteKyPth, string privteKyPassPhrase)
        {
            SftpClient sftp = null;
            int portNum = portNumber.HasValue ? portNumber.GetValueOrDefault() : 22;        // Set port 22 by default
            try
            {
                var connectionInfo = new ConnectionInfo(hostName, portNum, userName, new PasswordAuthenticationMethod(userName, password),
                    new PrivateKeyAuthenticationMethod(userName, new PrivateKeyFile(File.OpenRead(privteKyPth), privteKyPassPhrase)));

                objSftpClient = new SftpClient(connectionInfo);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Could not initilize Sftp object. Ex: {0}", ex.Message));
            }

        }

        public SftpHelper(string userName, string hostName, int? portNumber, string privteKyPth)
        {
            SftpClient sftp = null;
            int portNum = portNumber.HasValue ? portNumber.GetValueOrDefault() : 22;        // Set port 22 by default
            try
            {
                var connectionInfo = new ConnectionInfo(hostName, portNum, userName,
                      new PrivateKeyAuthenticationMethod(userName, new PrivateKeyFile(File.OpenRead(privteKyPth))));

                objSftpClient = new SftpClient(connectionInfo);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Could not initilize Sftp object. Ex: {0}", ex.Message));
            }

        }
        #endregion


        #region Download
        /// <summary>
        /// Downloads files at sftp folder location to temp folder path on server
        /// </summary>
        /// <param name="sftpFolderLocatn"></param>
        /// <param name="folderDownloadPath"></param>
        /// <param name="fileNmesToMtchStr"></param>
        /// <param name="deleteFileFrmSrver"></param>
        /// <returns>return false if failed to find any files at location otherwise true</returns>
        public bool DownloadFilesFromSftp(string sftpFolderLocatn, string folderDownloadPath, string fileNmesToMtchStr, bool deleteFileFrmSrver)
        {
            try
            {
                if (objSftpClient == null) return false;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    var csvFiles = objSftpClient.ListDirectory(sftpFolderLocatn).Where(x => x.Name.ToLower().IndexOf(fileNmesToMtchStr) != -1);
                    if (csvFiles == null) return false;

                    foreach (var file in csvFiles)
                    {
                        using (var stream = new FileStream(Path.Combine(folderDownloadPath, file.Name), FileMode.Create))
                        {
                            objSftpClient.DownloadFile(file.FullName, stream);
                        }
                        if (deleteFileFrmSrver)
                            objSftpClient.Delete(file.FullName);
                    }
                    
                }
                else
                {
                    throw new Exception("Error: GetFileFromSftp() - Could not connect to sftp server");
                }

            }
            catch (Exception ex)
            {
                throw  new Exception(String.Format("Error downloading files from sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();
            }

            return true;
        }

        /// <summary>
        /// Downloads files at sftp folder location to temp folder path on server
        /// </summary>
        /// <param name="sftpFolderLocatn"></param>
        /// <param name="folderDownloadPath"></param>
        /// <param name="deleteFileFrmSrver"></param>
        /// <returns>return false if failed to find any files at location otherwise true</returns>
        public bool DownloadFilesFromSftp(string sftpFolderLocatn, string folderDownloadPath, bool deleteFileFrmSrver)
        {
            try
            {
                if (objSftpClient == null) return false;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    var csvFiles = objSftpClient.ListDirectory(sftpFolderLocatn).Where(x => x.Name != "." && x.Name != "..");
                    if (csvFiles == null) return false;

                    foreach (var file in csvFiles)
                    {
                        using (var stream = new FileStream(Path.Combine(folderDownloadPath, file.Name), FileMode.Create))
                        {
                            objSftpClient.DownloadFile(file.FullName, stream);
                        }
                        if (deleteFileFrmSrver)
                            objSftpClient.Delete(file.FullName);
                    }

                }
                else
                {
                    throw new Exception("Error: GetFileFromSftp() - Could not connect to sftp server");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error downloading files from sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();
 
            }

            return true;
        }

        /// <summary>
        /// Downloads files at sftp folder location to temp folder path on server & returns a reference to the file paths
        /// </summary>
        /// <param name="sftpFolderLocatn"></param>
        /// <param name="folderDownloadPath"></param>
        /// <param name="deleteFileFrmSrver"></param>
        /// <returns>Return false if failed to find any files at location otherwise true</returns>
        public List<string> GetDownloadFilesFromSftp(string sftpFolderLocatn, string folderDownloadPath, bool deleteFileFrmSrver)
        {
            List<String> lstFilesInTemp = new List<string>();
            try
            {
                if (objSftpClient == null) return null;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    var csvFiles = objSftpClient.ListDirectory(sftpFolderLocatn).Where(x => x.Name != "." && x.Name != "..");
                    if (csvFiles == null) return null;

                    foreach (var file in csvFiles)
                    {
                        string fileDownloadPath = Path.Combine(folderDownloadPath, file.Name);
                        using (var stream = new FileStream(fileDownloadPath, FileMode.Create))
                        {
                            objSftpClient.DownloadFile(file.FullName, stream);
                        }

                        if (File.Exists(fileDownloadPath))
                            lstFilesInTemp.Add(fileDownloadPath);

                        if (deleteFileFrmSrver)
                            objSftpClient.Delete(file.FullName);
                    }

                }
                else
                {
                    throw new Exception("Error: GetDownloadFilesFromSftp() - Could not connect to sftp server");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error downloading files from sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();

            }

            return lstFilesInTemp;
        }

        /// <summary>
        /// Downloads files at sftp folder location to temp folder path on server & returns a reference to the file paths
        /// </summary>
        /// <param name="sftpFolderLocatn"></param>
        /// <param name="folderDownloadPath"></param>
        /// <param name="fileNmesToMtchStr">Filter by files matching parameter</param>
        /// <param name="deleteFileFrmSrver"></param>
        /// <returns>return false if failed to find any files at location otherwise true</returns>
        public List<string> GetDownloadFilesFromSftp(string sftpFolderLocatn, string folderDownloadPath, string fileNmesToMtchStr, bool deleteFileFrmSrver)
        {
            List<String> lstFilesInTemp = new List<string>();
            try
            {
                if (objSftpClient == null) return null;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    var csvFiles = objSftpClient.ListDirectory(sftpFolderLocatn).Where(x => x.Name.ToLower().Contains(fileNmesToMtchStr.ToLower()));
                    if (csvFiles == null) return null;

                    foreach (var file in csvFiles)
                    {
                        string fileDownloadPath = Path.Combine(folderDownloadPath, file.Name);
                        using (var stream = new FileStream(fileDownloadPath, FileMode.Create))
                        {
                            objSftpClient.DownloadFile(file.FullName, stream);
                        }

                        if (File.Exists(fileDownloadPath))
                            lstFilesInTemp.Add(fileDownloadPath);

                        if (deleteFileFrmSrver)
                            objSftpClient.Delete(file.FullName);
                    }

                }
                else
                {
                    throw new ProcessingException("Error: GetDownloadFilesFromSftp() - Could not connect to sftp server");
                }

            }
            catch (Exception ex)
            {
                throw new ProcessingException(String.Format("Error downloading files from sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();
            }

            return lstFilesInTemp;
        }

        /// <summary>
        /// Downloads files at sftp folder location to temp folder path on server & returns a reference to the file paths
        /// </summary>
        /// <param name="sftpFolderLocatn">The SFTP folder locatn.</param>
        /// <param name="folderDownloadPath">The folder download path.</param>
        /// <param name="fileNmesToMtchStr">The file nmes to MTCH string.</param>
        /// <param name="extensionToContain">The extension to contain.</param>
        /// <param name="deleteFileFrmSrver">if set to <c>true</c> [delete file FRM srver].</param>
        /// <returns></returns>
        /// <exception cref="ProcessingException">
        /// Error: GetDownloadFilesFromSftp() - Could not connect to sftp server
        /// or
        /// </exception>
        public List<string> GetDownloadFilesFromSftp(string sftpFolderLocatn, string folderDownloadPath, string fileNmesToMtchStr, string extensionToContain, bool deleteFileFrmSrver)
        {
            List<String> lstFilesInTemp = new List<string>();
            try
            {
                if (objSftpClient == null) return null;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    var csvFiles = objSftpClient.ListDirectory(sftpFolderLocatn).Where(x => x.Name.ToLower().Contains(fileNmesToMtchStr.ToLower()));
                    if (csvFiles == null) return null;

                    foreach (var file in csvFiles)
                    {
                        string extension = Path.GetExtension(file.Name);
                        if (extension.Contains(extensionToContain))
                        {
                            string fileDownloadPath = Path.Combine(folderDownloadPath, file.Name);
                            using (var stream = new FileStream(fileDownloadPath, FileMode.Create))
                            {
                                objSftpClient.DownloadFile(file.FullName, stream);
                            }

                            if (File.Exists(fileDownloadPath))
                                lstFilesInTemp.Add(fileDownloadPath);

                            if (deleteFileFrmSrver)
                                objSftpClient.Delete(file.FullName);
                        }
                    }

                }
                else
                {
                    throw new ProcessingException("Error: GetDownloadFilesFromSftp() - Could not connect to sftp server");
                }

            }
            catch (Exception ex)
            {
                throw new ProcessingException(String.Format("Error downloading files from sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();
            }

            return lstFilesInTemp;
        }
        #endregion


        #region Upload
        /// <summary>
        /// Upload array of files to SFTP server.
        /// specifiying sftp folder location to upload too
        /// </summary>
        /// <param name="sftpFolderLocatn"></param>
        /// <param name="filesToUpload"></param>
        /// <returns></returns>
        public bool UploadFilesToSftpSrver(string sftpFolderLocatn, string[] filesToUpload)
        {
            try
            {
                if (objSftpClient == null) return false;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    objSftpClient.ChangeDirectory(sftpFolderLocatn);
                    if (filesToUpload == null) return false;

                    foreach (var file in filesToUpload)
                    {
                        using (var fileStream = File.OpenRead(file))
                        {
                            // Upload files to SFTP server
                            objSftpClient.BufferSize = 4*1024;
                            objSftpClient.UploadFile(fileStream, Path.GetFileName(file));
                        }
                    }

                }
                else
                {
                    throw new Exception("Error: UploadFilesToSftpSrver() - Could not connect to sftp server");
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Exception - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                throw new Exception(String.Format("Error uploading files to sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();
            }

            return true;

        }

        /// <summary>
        /// Upload array of files to SFTP server.
        /// Uses sftp folder path set in constructor to upload too
        /// </summary>
        /// <param name="filesToUpload"></param>
        /// <returns></returns>
        public bool UploadFilesToSftpSrver(string[] filesToUpload)
        {
            try
            {
                if (objSftpClient == null) return false;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    if (filesToUpload == null) return false;

                    foreach (var file in filesToUpload)
                    {
                        using (var fileStream = File.OpenRead(file))
                        {
                            // Upload files to SFTP server
                            objSftpClient.BufferSize = 4 * 1024;
                            objSftpClient.UploadFile(fileStream, Path.GetFileName(file));
                        }
                    }

                }
                else
                {
                    throw new Exception("Error: UploadFilesToSftpSrver() - Could not connect to sftp server");
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Exception - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                throw new Exception(String.Format("Error uploading files to sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();
            }

            return true;

        }

        /// <summary>
        /// Upload a file from the SFTP server
        /// </summary>
        /// <param name="fileToUpload"></param>
        /// <returns></returns>
        public bool UploadFileToSftpSrver(string fileToUpload)
        {
            try
            {
                if (objSftpClient == null) return false;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    if (fileToUpload == null) return false;
                    
                    using (var fileStream = File.OpenRead(fileToUpload))
                    {
                        // Upload files to SFTP server
                        objSftpClient.BufferSize = 4 * 1024;
                        objSftpClient.UploadFile(fileStream, Path.GetFileName(fileToUpload));
                    }
                }
                else
                {
                    throw new Exception("Error: UploadFilesToSftpSrver() - Could not connect to sftp server");
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Exception - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                throw new Exception(String.Format("Error uploading files to sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();
            }

            return true;

        }

        /// <summary>
        /// Upload a file from the SFTP server
        /// </summary>
        /// <param name="fileToUpload"></param>
        /// <returns></returns>
        public bool UploadFileToSftpSrver(string sftpFolderLocatn, string fileToUpload)
        {
            try
            {
                if (objSftpClient == null) return false;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    objSftpClient.ChangeDirectory(sftpFolderLocatn);
                    if (fileToUpload == null) return false;

                    using (var fileStream = File.OpenRead(fileToUpload))
                    {
                        // Upload files to SFTP server
                        objSftpClient.BufferSize = 4 * 1024;
                        objSftpClient.UploadFile(fileStream, Path.GetFileName(fileToUpload));
                    }
                }
                else
                {
                    throw new Exception("Error: UploadFilesToSftpSrver() - Could not connect to sftp server");
                }
            }
            catch (Exception ex)
            {
               // Log.Error(String.Format("Exception - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                throw new Exception(String.Format("Error uploading files to sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();
            }

            return true;

        }
        #endregion

        public bool WatchSftpDir(string ftpFolderLocation, string downLoadPath, string filePattern, int timeout)
        {
            try
            {
                // Iterate until
                bool fileFound = false;
                int currTime = 0;
                while (!fileFound)
                {
                    if (GetDownloadFilesFromSftp(ftpFolderLocation, downLoadPath, filePattern, true).Count > 0)
                        break;

                    if (currTime > timeout)
                        throw new TimeoutException(String.Format("Timeout occured waiting for APDM report file. Based on upload file name: {0}", filePattern));
                    else
                        Thread.Sleep(60000); // 1 min

                    currTime++;
                }

            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Exception - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                return false;
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();
            }

            return true;
        }

        #region Delete
        /// <summary>
        /// Delete files from SFTP server that match string names in array
        /// Using currently set working directory
        /// </summary>
        /// <param name="arrFileNmesToMtchStr"></param>
        /// <returns></returns>
        public bool DeleteFilesFromServer(string[] arrFileNmesToMtchStr)
        {
            try
            {
                if (objSftpClient == null) return false;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    if (arrFileNmesToMtchStr == null) return false;
                    foreach (string fileToDelete in arrFileNmesToMtchStr)
                    {
                        var csvFiles = objSftpClient.ListDirectory(objSftpClient.WorkingDirectory).Where(x => x.Name.ToLower().IndexOf(fileToDelete.ToLower()) != -1);
                        if (csvFiles != null)
                        {
                            foreach (var csvFile in csvFiles)
                            {
                                objSftpClient.Delete(csvFile.FullName);
                            }
                           
                        }
                    }
                  
                }
                else
                {
                    throw new Exception("Error: DeleteFilesFromServer() - Could not connect to sftp server");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error Deleting files from sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();

            }

            return true;
        }

        /// <summary>
        /// Delete files from SFTP server that match string names in array
        /// Explicitly setting folder to delete from
        /// </summary>
        /// <param name="arrFileNmesToMtchStr"></param>
        /// <param name="fileNmesToMtchStr"></param>
        /// <returns></returns>
        public bool DeleteFilesFromServer(string sftpFolderLocatn, string[] arrFileNmesToMtchStr)
        {
            try
            {
                if (objSftpClient == null) return false;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    if (arrFileNmesToMtchStr == null) return false;
                    foreach (string fileToDelete in arrFileNmesToMtchStr)
                    {
                        var csvFiles = objSftpClient.ListDirectory(sftpFolderLocatn).Where(x => x.Name.ToLower().IndexOf(fileToDelete.ToLower()) != -1);
                        if (csvFiles == null) return false;

                        foreach (var file in csvFiles)
                        {
                            objSftpClient.Delete(file.FullName);
                        }
                    }

                }
                else
                {
                    throw new Exception("Error: DeleteFilesFromServer() - Could not connect to sftp server");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error Deleting files from sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();

            }

            return true;
        }

        /// <summary>
        /// Delete a single file from SFTP server that matches the string name
        /// Explicitly setting folder to delete from
        /// </summary>
        /// <param name="sftpFolderLocatn"></param>
        /// <param name="fileNmesToMtchStr"></param>
        /// <returns></returns>
        public bool DeleteFileFromServer(string sftpFolderLocatn, string fileNmesToMtchStr)
        {
            try
            {
                if (objSftpClient == null) return false;
                objSftpClient.Connect();
                if (objSftpClient.IsConnected)
                {
                    if (fileNmesToMtchStr == null) return false;

                    var csvFile = objSftpClient.ListDirectory(sftpFolderLocatn).Where(x => x.Name.ToLower().IndexOf(fileNmesToMtchStr.ToLower()) != -1).SingleOrDefault();
                    if (csvFile == null) return false;
                    objSftpClient.Delete(csvFile.FullName);
                    
                }
                else
                {
                    throw new Exception("Error: DeleteFileFromServer() - Could not connect to sftp server");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error Deleting files from sftp server. Ex: {0}", ex.Message));
            }
            finally
            {
                if (objSftpClient != null)
                    if (objSftpClient.IsConnected)
                        objSftpClient.Disconnect();

            }

            return true;
        }
        #endregion




        /// <summary>
        /// Return Sftp object using connection parameters
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="hostName"></param>
        /// <param name="portNumber">Port 22 is used by default if null</param>
        /// <returns></returns>
        [Obsolete]
        public static SftpClient SetSFTPClientDetails(string userName, string password, string hostName, int? portNumber)
        {
            SftpClient sftp = null;
            int portNum = portNumber.HasValue ? portNumber.GetValueOrDefault() : 22;        // Set port 22 by default
            try
            {
                var connectionInfo = new ConnectionInfo(hostName, portNum, userName, new PasswordAuthenticationMethod(userName, password));

                sftp = new SftpClient(connectionInfo);
            }
            catch (Exception ex)
            {
                // Returning null value
            }

            return sftp;
        }

        /// <summary>
        /// Return Sftp object using connection parameters
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="hostName"></param>
        /// <param name="portNumber">Port 22 is used by default if null</param>
        /// <param name="privteKyPth"></param>
        /// <param name="privteKyPassPhrase"></param>
        /// <returns></returns>
        [Obsolete]
        public static SftpClient SetSFTPClientDetails(string userName, string password, string hostName, int? portNumber, string privteKyPth, string privteKyPassPhrase)
        {
            SftpClient sftp = null;
            int portNum = portNumber.HasValue ? portNumber.GetValueOrDefault() : 22;        // Set port 22 by default
            try
            {
                var connectionInfo = new ConnectionInfo(hostName, portNum, userName, new PasswordAuthenticationMethod(userName, password),
                     new PrivateKeyAuthenticationMethod(userName, new PrivateKeyFile(File.OpenRead(privteKyPth), privteKyPassPhrase)));

                sftp = new SftpClient(connectionInfo);
            }
            catch (Exception ex)
            {
                // Returning null value
            }

            return sftp;
        }
    }
}
