using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;




namespace CommonLib
{
    public class FTPSHelper
    {
        public bool UploadFile(string ftpsHostname, string ftpsDir, string userName, string password, string fileToUpload)
        {
            // baseUri format should be: ftp:// IP or ftpServerName / dir
            // serverUri format should be: baseUri / filename
            Uri baseUri = new Uri(String.Format("ftp://{0}/{1}/", ftpsHostname, ftpsDir));
            FileInfo fileInfo = new FileInfo(fileToUpload);
            string fileName = fileInfo.Name;
            Uri ftpsUri = new Uri(baseUri, fileName);

            // serverUri format should be: ftp:// + IP or ftpServerName + / filename
            if (ftpsUri.Scheme != Uri.UriSchemeFtp)
                return false;

            try
            {
                using (var fs = File.OpenRead(fileToUpload))
                {

                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpsUri);
                    request.EnableSsl = true;
                    // This statement is to ignore certification validation warning
                    System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    request.Credentials = new NetworkCredential(userName, password);
                    request.KeepAlive = true;
                    request.UseBinary = true;
                    request.Timeout = -1;
                    request.Method = WebRequestMethods.Ftp.UploadFile;

                    Stream ftpstream = request.GetRequestStream();
                    fs.CopyTo(ftpstream); // Copy the FileStream to the FTP RequestStream
                    ftpstream.Close();
                    //check the response
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                }

            }
            catch (Exception ex)
            {
                throw new ProcessingException(String.Format("Error uploading files to ftps server. Ex: {0}", ex.Message));

            }
            return true;
        }

        public List<string> GetFilesList(string ftpsHostname, string userName, string password, string ftpsDir)
        {
            List<String> lstFiles = new List<string>();
            try
            {

                Uri baseUri = new Uri(String.Format("ftp://{0}/{1}/", ftpsHostname, ftpsDir));
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(baseUri);
                request.EnableSsl = true;
                // This statement is to ignore certification validation warning
                System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                request.Credentials = new NetworkCredential(userName, password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    lstFiles.Add(line);
                    line = reader.ReadLine();
                }

                reader.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new ProcessingException(String.Format("Error getting list of files from ftps server. Ex: {0}", ex.Message));
            }
            return lstFiles;

        }

        #region Download
        public bool DownloadFile(string ftpsHostname, string userName, string password, string ftpsDir, string destDir, string targetFile, bool deleteAfterDownload)
        {

            try
            {

                Uri baseUri = new Uri(String.Format("ftp://{0}/{1}/{2}", ftpsHostname, ftpsDir, targetFile));
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(baseUri);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.EnableSsl = true;
                // This statement is to ignore certification validation warning
                System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                request.Credentials = new NetworkCredential(userName, password);

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();


                FileStream writeStream = new FileStream(destDir + "\\" + targetFile, FileMode.Create);
                int Length = 2048;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);
                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                }

                if (deleteAfterDownload)
                    DeleteFile(ftpsHostname, userName, password, ftpsDir, targetFile);

                writeStream.Close();
                response.Close();

                return true;
            }
            catch (Exception ex)
            {
                throw new ProcessingException(String.Format("Error downloading file from ftps server. Ex: {0}", ex.Message));
            }
        }

        public bool DownloadFiles(string ftpsHostname, string userName, string password, string ftpsDir, string destDir, bool deleteAfterDownload)
        {
            try
            {
                var files = GetFilesList(ftpsHostname, userName, password, ftpsDir);
                foreach (var file in files)
                {
                    Uri baseUri = new Uri(String.Format("ftp://{0}/{1}/{2}", ftpsHostname, ftpsDir, file));
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(baseUri);
                    request.EnableSsl = true;
                    // This statement is to ignore certification validation warning
                    System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    request.Credentials = new NetworkCredential(userName, password);
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();


                    FileStream writeStream = new FileStream(destDir + "\\" + file, FileMode.Create);
                    int Length = 2048;
                    Byte[] buffer = new Byte[Length];
                    int bytesRead = responseStream.Read(buffer, 0, Length);
                    while (bytesRead > 0)
                    {
                        writeStream.Write(buffer, 0, bytesRead);
                        bytesRead = responseStream.Read(buffer, 0, Length);
                    }

                    if (deleteAfterDownload)
                        DeleteFile(ftpsHostname, userName, password, ftpsDir, file);

                    writeStream.Close();
                    response.Close();

                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ProcessingException(String.Format("Error downloading files from ftps server. Ex: {0}", ex.Message));
            }

        }
        public List<string> GetDownloadFiles(string ftpsHostname, string userName, string password, string ftpsDir, string destDir, bool deleteAfterDownload)
        {
            List<String> lstFiles = new List<string>();
            try
            {
                var files = GetFilesList(ftpsHostname, userName, password, ftpsDir);
                foreach (var file in files)
                {
                    lstFiles.Add(file);
                    Uri baseUri = new Uri(String.Format("ftp://{0}/{1}/{2}", ftpsHostname, ftpsDir, file));
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(baseUri);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;

                    request.Credentials = new NetworkCredential(userName, password);
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();


                    FileStream writeStream = new FileStream(destDir + "\\" + file, FileMode.Create);
                    int Length = 2048;
                    Byte[] buffer = new Byte[Length];
                    int bytesRead = responseStream.Read(buffer, 0, Length);
                    while (bytesRead > 0)
                    {
                        writeStream.Write(buffer, 0, bytesRead);
                        bytesRead = responseStream.Read(buffer, 0, Length);
                    }

                    if (deleteAfterDownload)
                        DeleteFile(ftpsHostname, userName, password, ftpsDir, file);

                    writeStream.Close();
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ProcessingException(String.Format("Error downloading files List from ftps server. Ex: {0}", ex.Message));
            }

            return lstFiles;
        }
        #endregion


        #region Delete
        public bool DeleteFile(string ftpsHostname, string userName, string password, string ftpsDir, string targetFile)
        {

            try
            {

                Uri baseUri = new Uri(String.Format("ftp://{0}/{1}/{2}", ftpsHostname, ftpsDir, targetFile));
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(baseUri);
                request.Credentials = new NetworkCredential(userName, password);

                request.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();

                return true;
            }
            catch (Exception ex)
            {
                throw new ProcessingException(String.Format("Error deleting file from ftps server. Ex: {0}", ex.Message));
            }
        }
        #endregion

    }
}
