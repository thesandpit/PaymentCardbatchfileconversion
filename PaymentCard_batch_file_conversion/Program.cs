using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommonLib;



using System.IO;

namespace PaymentCard_batch_file_conversion
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"\\doc2\Prod\Simply_Energy\PaymentCards";
            string binDir = Path.Combine(filePath, "bin");
            string spoolDir = Path.Combine(filePath, "spool");
            string PClog = binDir + "\\log.txt";
            string indexFile = "index.csv";
            string dateStrPath = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString("D2") + DateTime.Today.Day.ToString();
            string archiveDir = "PC-Larch_" + dateStrPath;
            string archiveDirPath = Path.Combine(spoolDir, archiveDir);

            //Routines to delete old/previous data and spoolfiles and prepare new data for processing
            preProcessingSteps(filePath, binDir, PClog);

            try
            {
                File.OpenWrite(PClog);
                File.AppendAllText(PClog, "SE-PAYMENT CARD Processing Job Started: " + DateTime.Now);
                File.AppendAllText(PClog, "-----");
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
            }
            ////Routines to start processing
            SEPaymentCardProcessing(PClog, binDir, filePath);

            //Routines to do Archiving
            SEPaymentCardArchiving(archiveDirPath, spoolDir, archiveDir, indexFile);

            try
            {
                File.AppendAllText(PClog, "-----");
                File.AppendAllText(PClog, "SE-PAYMENT CARD Processing Job Finished: " + DateTime.Now);
                File.Copy(PClog, spoolDir + "\\PC_log_" + dateStrPath);
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
            }
            Console.ReadLine();            
        }

        public static void AutoShrinkProg1(string filePath)
        {
            FileProcessHelper.DeleteFiles(@"\\prog1\PDFwork2\simplyPC", "*.pdf");
            FileProcessHelper.CopyFiles(@"\\DOC2\Prod\Simply_Energy\PaymentCards\spool", @"\\prog1\PDFwork2\simplyPC", "*.pdf");
            RunProcessHelper.RunProcess(true, @"\\prog1\Fdrive\utl\PSexec.exe", "\"\\prog1 - u zipform\\elixir - p zipform\" d:\\PDFwork\\AutoBatch SEPC_Shrink.bat");
            FileProcessHelper.CopyFiles(@"\\prog1\PDFwork2\simplyPC", @"\\DOC2\Prod\Simply_Energy\PaymentCards\spool", "*.pdf");
            FileProcessHelper.DeleteFiles(@"\\prog1\PDFwork2\simplyPC", "*.pdf");            
        }

        public static void preProcessingSteps(string filePath, string binDir, string PClog)
        {
            string[] dirToLookIn = new string[] { "\\spool", "\\data" };
            string[][] FileTypesToDelete = { new string[] { "pdf", "csv", "txt", "jrn", "zip" }, new string[] { "bsp", "dbf", "dpi", "sum", "trf", "txt", "ntx" } };

            for (int i = 0; i < 2; i++)
            {
                foreach (string extension in FileTypesToDelete[i])
                    FileProcessHelper.DeleteFiles(filePath + dirToLookIn[i], "*." + extension);
            }

            //TODO: IO operations should use try / catch or within wrapper methods when dealing with as considered unsafe operations
            FileProcessHelper.AppendFiles(filePath + "\\in", @"\\doc2\Prod\Simply_Energy\PaymentCards\data\EA_EAPRD_EZYPAY.TXT", "EA*.txt", false);
            RunProcessHelper.RunProcess(true, filePath + @"\data\ZFPROC200pipeDOC2.exe", "/T");

            FileProcessHelper.DeleteFile(PClog);
            FileProcessHelper.DeleteFile(binDir + "\\Dtotal.txt");
            FileProcessHelper.DeleteFile(binDir + "\\pages.txt");
            FileProcessHelper.CopyFile(binDir + "\\blankpages.txt", binDir, "pages.txt");
        }

        public static void SEPaymentCardProcessing(string PClog, string binDir, string filePath)
        {           
            //DOC1GENA Routines
            RunProcessHelper.RunProcess(true, @"\\Doc2\Doc1\Generate\doc1gen.exe", @"D:\Prod\Simply_Energy\PaymentCards\bin\PaycardsArch.hip\"" OPS=D:\Prod\Simply_Energy\PaymentCards\bin\DOC1fileA.OPS\""  >> \\doc2\Prod\Simply_Energy\PaymentCards\bin\Error.log");
            FileProcessHelper.AppendFile(binDir + "\\Dtotal.txt", PClog, false);
            //Shrink PDF routine
            AutoShrinkProg1(filePath);
        }

        public static void SEPaymentCardArchiving(string archiveDirPath, string spoolDir, string archiveDir, string indexFile)
        {
            //TODO: nothing wrong here just not very readable, could refactor to below
            //Store current date in yyyymmdd format                    

            //creates Directory with naming format "PC-Larch_yyyymmdd"            
            FileProcessHelper.CreateDirectory(archiveDirPath);
            FileProcessHelper.CreateDirectory(Path.Combine(archiveDirPath, archiveDir));
            //moves PDF files into the "PC-Larch_yyyymmdd" directory
            FileProcessHelper.MoveFiles(spoolDir, Path.Combine(archiveDirPath, archiveDir), "*.pdf");
            FileProcessHelper.MoveFile(spoolDir + "\\" + indexFile, Path.Combine(archiveDirPath, archiveDir), indexFile);


            //ZIPPING ROUTINES
            FileProcessHelper.ZipFolder(archiveDirPath, @"T:\Prod_BU\Simply_Energy\To_Send", archiveDir, false, false);                     
            FileProcessHelper.ZipFolder(archiveDirPath, spoolDir, archiveDir, false, false);       
            //FileProcessHelper.ZipFiles(spoolDir, "", archiveDir + ".zip");            
            //TODO: Safer to use Path.Combine(filePath, "\spool","\\PC-Larch_"); when combing paths, will automatically cater for missing '\'
            FileProcessHelper.MoveFiles(archiveDirPath, spoolDir, "*.pdf");
            FileProcessHelper.MoveFile(archiveDirPath + "\\" + indexFile, spoolDir, indexFile);

            FileProcessHelper.DeleteFolders(spoolDir, "PC*");
            //FileProcessHelper.DeleteFolders(spoolDir, archiveDir);                       

            FileProcessHelper.DeleteFiles(spoolDir, "*.pdf");
            FileProcessHelper.DeleteFile(spoolDir + "\\" + indexFile);
        }
    }
}
