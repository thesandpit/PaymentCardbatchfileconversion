using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class ZipFileHelper
    {
        public static string UnZipFiles(string sourcefile, string destDir)
        {

            try
            {
                var fileName = new FileInfo(sourcefile).Name;
                var extractFolderName = fileName.Split('.')[0];
                var extractDir = Path.Combine(destDir, extractFolderName);
                if (Directory.Exists(extractDir))
                    Directory.Delete(extractDir, true);
         
                ZipFile.ExtractToDirectory(Path.Combine(destDir, sourcefile), extractDir);

                return extractDir;
            }
            catch (Exception ex)
            {
                throw new ProcessingException(String.Format("Failed to unzip {1}. Ex: {0}", ex.Message, sourcefile));
            }

        }

    }
}
