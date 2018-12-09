using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace CommonLib
{
    public class FileProcessHelper
    {

       

        #region Append
        /// <summary>
        /// Append a sourceFile to a destFile 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <param name="deleteAfterAppend">if delete the source file after append</param>
        /// <returns></returns>
        public static void AppendFile(string sourceFile, string destFile, bool deleteAfterAppend)
        {
            try
            {
                //string[] fileList = pattern == null ? Directory.GetFiles(sourcePath) : Directory.GetFiles(sourcePath, pattern);
                //foreach (string sourceFile in fileList)
                //{
                using (Stream input = File.OpenRead(sourceFile))
                using (Stream output = new FileStream(destFile, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    input.CopyTo(output);
                }

                if (deleteAfterAppend)
                    DeleteFile(sourceFile);
                //}
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
                throw new ProcessingException(string.Format("Fail to append {0} file to {1}", sourceFile, destFile));
            }
        }

        /// <summary>
        /// Append files from sourcePath to a destPath 
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destFile"></param>
        /// <param name="pattern"></param>
        /// <param name="deleteAfterAppend"></param>
        /// <returns></returns>
        public static void AppendFiles(string sourcePath, string destFile, string pattern, bool deleteAfterAppend)
        {
            try
            {
                string[] fileList = pattern == null ? Directory.GetFiles(sourcePath) : Directory.GetFiles(sourcePath, pattern);
                foreach (string sourceFile in fileList)
                {
                    using (Stream input = File.OpenRead(sourceFile))
                    using (Stream output = new FileStream(destFile, FileMode.Append, FileAccess.Write, FileShare.None))
                    {
                        input.CopyTo(output);
                    }
                    if (deleteAfterAppend)
                        DeleteFile(sourceFile);
                }
            }
            catch (Exception ex)
            {
                Log.Info(MethodBase.GetCurrentMethod().Name + ex.Message);
                throw new ProcessingException(string.Format("Fail to append files from {0} to {1}", sourcePath, destFile));
            }

        }
        #endregion


        #region Copy
        /// <summary>
        /// Copy an existing file to a new destpath 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destPath"></param>
        /// <param name="newFileName">if not pass newFileName, it will use the sourcefileName</param>
        /// <returns></returns>
        public static void CopyFile(string sourceFile, string destPath, string newFileName)
        {
            bool isSuccess = false;
            try
            {
                string sourceFileName = Path.GetFileName(sourceFile);
                string destFile = newFileName == null ? Path.Combine(destPath, sourceFileName) : Path.Combine(destPath, newFileName);
                File.Copy(sourceFile, destFile, true);

                if (File.Exists(destFile))
                    isSuccess = true;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                isSuccess = false;
            }
            if (!isSuccess)
                throw new ProcessingException(string.Format("Fail to copy file {0} to {1} ", sourceFile, destPath));
        }

        /// <summary>
        /// Copy existing files to a destpath
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="pattern">search pattern</param>
        /// <returns></returns>
        public static void CopyFiles(string sourcePath, string destPath, string pattern)
        {
            string sourceFileName = null;
            string destFile = null;
            int counter = 0;
            bool isSuccess = false;

            try
            {
                string[] fileList = Directory.GetFiles(sourcePath, pattern);
                foreach (string sourceFile in fileList)
                {
                    sourceFileName = Path.GetFileName(sourceFile);
                    destFile = Path.Combine(destPath, sourceFileName);
                    File.Copy(sourceFile, destFile, true);
                    counter++;
                }
                if (counter == fileList.Count())
                    isSuccess = true;

            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                isSuccess = false;
            }

            if (!isSuccess)
                throw new ProcessingException(string.Format("Fail to copy files from {0} to {1} ", sourcePath, destPath));
        }
        #endregion


        #region Delete
        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="targetFile"></param>
        /// <returns></returns>
        public static void DeleteFile(string targetFile)
        {
            bool isSuccess = false;
            if (File.Exists(targetFile))
            {
                try
                {
                    File.Delete(targetFile);
                    if (!File.Exists(targetFile))
                        isSuccess = true;
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                }
            }
            else
            {
                isSuccess = true;
            }

            if (!isSuccess)
                throw new ProcessingException(string.Format("Fail to delete {0}", targetFile));
        }

        /// <summary>
        /// Delete files from a path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pattern">search pattern</param>
        /// <returns></returns>
        public static void DeleteFiles(string path, string pattern)
        {
            bool isSuccess = false;
            try
            {
                string[] fileList = Directory.GetFiles(path, pattern);
                foreach (string file in fileList)
                {
                    File.Delete(file);
                }

                string[] fileListAfterDelete = Directory.GetFiles(path, pattern);
                if (fileListAfterDelete.Count() == 0)
                    isSuccess = true;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                isSuccess = false;
            }

            if (!isSuccess)
                throw new ProcessingException(string.Format("Fail to delete files from {0}", path));
        }


        public static void DeleteFolders(string path, string pattern)
        {
            bool isSuccess = false;
            try
            {
                string[] folderList = Directory.GetDirectories(path, pattern);
                foreach (string folder in folderList)
                {
                    Directory.Delete(folder, true);
                }
                string[] FolderListAfterDelete = Directory.GetDirectories(path, pattern);
                if (FolderListAfterDelete.Count() == 0)
                    isSuccess = true;
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
                isSuccess = false;
            }

            if (!isSuccess)
                throw new ProcessingException(String.Format("Fail to delete folders from {0}", path));
        }
          
        #endregion


        #region Get
        public static string[] GetFolders(string path, string pattern)
        {
            string[] folders = null;
            try
            {
                folders = pattern == null ? Directory.GetDirectories(path) : Directory.GetDirectories(path, pattern);
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
                throw new ProcessingException(String.Format("Fail to get files from {0}", path));
            }
            return folders;
        }

        public static string[] GetFiles(string path, string pattern)
        {
            string[] files = null;
            try
            {
                files = pattern == null ? Directory.GetFiles(path) : Directory.GetFiles(path, pattern);
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                throw new ProcessingException(String.Format("Fail to get files from {0}", path));
            }
            return files;
        }

        public static string GetFirstFile(string path, string pattern)
        {
            string files = String.Empty;
            try
            {
                files = pattern == null ? Directory.GetFiles(path)?[0] : Directory.GetFiles(path, pattern)?[0];
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                throw new ProcessingException(String.Format("Fail to get file from {0}", path));
            }
            return files;
        }
        #endregion


        #region Move
        // Note: if not pass newFileName, it will use the sourcefileName
        public static bool MoveFile(string sourceFile, string destPath, string newFileName)
        {
            try
            {
                string sourceFileName = Path.GetFileName(sourceFile);
                string destFile = String.IsNullOrEmpty(newFileName) ? Path.Combine(destPath, sourceFileName) : Path.Combine(destPath, newFileName);
                if (File.Exists(destFile)) File.Delete(destFile);       // Exception if it exists already : delete then copy newer file
                    File.Move(sourceFile, destFile);
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                return false;
            }
            return true;
        }

        public static bool MoveFiles(string sourcePath, string destPath, string pattern)
        {
            string sourcefileName = null;
            string destFile = null;
            int counter = 0;
            bool isSuccess = false;
            try
            {
                string[] fileList = Directory.GetFiles(sourcePath,pattern);
                foreach (string sourceFile in fileList)
                {
                    sourcefileName = Path.GetFileName(sourceFile);
                    destFile = Path.Combine(destPath, sourcefileName);

                    if (File.Exists(destFile))
                    {
                        File.Copy(sourceFile, destFile, true);
                        File.Delete(sourceFile);
                    }
                    else
                    {
                         File.Move(sourceFile, destFile);
                    }
                    counter++;
             
                }

                if (fileList.Count() == counter)
                    isSuccess = true;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                isSuccess = false;
            }

            if (!isSuccess)
                throw new ProcessingException(string.Format("Fail to move file from {0} to {1}", sourcePath, destPath));

            return isSuccess;
        }
        #endregion

        #region RenameFiles

        public static bool RenameFile(string sourceFile, string oldFileName, string newFileName)
        {
            try
            {
                string newFullPath = Path.Combine(sourceFile, newFileName);
                File.Move(oldFileName, newFullPath);
                if (!File.Exists(newFullPath)) return false;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Zips files at source location matching extension. Saves zipfile using zipFileName parameter saved in the source location
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="extension">The extension.</param>
        /// <param name="zipFileName">Name of the zip file.</param>
        /// <returns></returns>
        public static bool ZipFiles(string sourcePath, string extension, string zipFileName)
        {
            try
            {
                string zipFilePath = Path.Combine(sourcePath, zipFileName);
                ZipArchive zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
                string[] fileList = Directory.GetFiles(sourcePath, "*" + extension);
                foreach (string file in fileList)
                {
                    if (!file.Equals(zipFilePath))     // Exclude the newly created zip from including in the zip.... 
                        zipArchive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                }
                zipArchive.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0}. Ex: {1}",ex.Message, MethodBase.GetCurrentMethod().Name));
                return false;
            }

            return true;
        }



        #endregion


        #region zip
        public static void ZipFolder(string sourceDir, string destDir, string zipFileName, bool addDateStamp, bool deleteAfterZip)
        {
            try
            {

                var zipfile = addDateStamp? Path.Combine(destDir, string.Format("{0}_{1}.zip",zipFileName, DateTime.Today.ToString("yyyyMMdd"))): Path.Combine(destDir, zipFileName + ".zip");

                using (var zip = new Ionic.Zip.ZipFile())
                {
                    zip.UseZip64WhenSaving = Ionic.Zip.Zip64Option.AsNecessary;
                    zip.AddDirectory(sourceDir);
                    zip.Save(zipfile);
                }
                if(deleteAfterZip)
                    FileProcessHelper.DeleteFiles(sourceDir, "*");
            }
            catch(Exception ex)
            {
                Log.Fatal(string.Format("Fail to zip folder {0}. ex:{1}", sourceDir, ex.ToString()));
            }

        }
        #endregion


        #region Create, Empty Directory
        public static void CreateDirectory(string path)
        {
            try
            {
                  if (!Directory.Exists(path))
                  Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                throw new ProcessingException(string.Format("Fail to create {0} ", path));
            }
     
        }

        public static void CheckDirectoryEmpty(string path)
        {
            bool isSuccess = false;
            try
            {
        
                int counter1 = FileProcessHelper.GetFolders(path, null).Count();
                int counter2 = FileProcessHelper.GetFiles(path, "*").Count();
                if (counter1 == 0 && counter2 == 0)
                    isSuccess = true;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error in method - {0} Ex: {1}", MethodBase.GetCurrentMethod().Name, ex.Message));
                isSuccess = false;
            }

            if (!isSuccess)
                throw new ProcessingException(string.Format("{0} is not empty", path));

        }
        #endregion
    }
}
