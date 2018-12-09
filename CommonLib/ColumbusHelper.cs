using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CsvHelper.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Diagnostics;
using System.Configuration;
using CsvHelper;

namespace CommonLib
{
    public class ColumbusHelper
    {

        #region Columbus Index Mapping
        public class ColumbusIndex
        {
            public string UniqueID { get; set; }
            private string _DocRefNo { get; set; }
            public string DocRefNo
            {
                get
                {
                    return _DocRefNo.Replace(" ", "");
                }
                set
                {
                    _DocRefNo = value;
                }
            }
            private string _CustomerNo { get; set; }
            public string CustomerNo
            {
                get
                {
                    return _CustomerNo.Replace(" ", "");
                }
                set
                {
                    _CustomerNo = value;
                }
            }
            private string _AccountNo { get; set; }
            public string AccountNo
            {
                get
                {
                    return _AccountNo.Replace(" ", "");
                }
                set
                {
                    _AccountNo = value;
                }
            }
            public string FirstName { get; set; }
            public string Surname { get; set; }
            public string CompanyName { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string AddressLine3 { get; set; }
            public string Suburb { get; set; }
            public string State { get; set; }
            public string Postcode { get; set; }
            public string EmailAddress { get; set; }
            public string DocDueDate { get; set; }
            public string EnergyType { get; set; }
            public int DocType { get; set; }
            public string DocMedia { get; set; }
            public string NMI { get; set; }
            public string MIRN { get; set; }
            public string ContractName { get; set; }
            public string SalesChannelID { get; set; }
            public string CurrentStatus { get; set; }
            public string InvoiceNumber { get; set; }
            public string IssueDate { get; set; }
            public string FullAmountDue { get; set; }
            public string DiscAmountDue { get; set; }
            public string MarketingSegment { get; set; }
            public string OfferCode { get; set; }
            public string BillingPeriodFrom { get; set; }
            public string BillingPeriodTo { get; set; }
            public string AvgDailyUsage { get; set; }
            public string Distributor { get; set; }
            public string Additional { get; set; }
            public int Pages { get; set; }

            //Extra 
            public string FileName { get; set; }
            public bool FileMissing { get; set; }
            public byte[] PdfTempData { get; set; }
            public bool FileEnriched { get; set; }
            public bool FileExpired { get; set; }
        } 
        #endregion

        public static bool ArchivePDFsToColumbus(string folderInput, string folderOutput, string columbusFilePrefix, List<ColumbusIndex> documentsToMerge)
        {
            try
            {

                //Merge Pdfs and create xml control files
                Log.Info(string.Format("{0} pdfs are ready to merge ", documentsToMerge.Count()));
                int fileNo = 1;
                while (documentsToMerge.Count != 0)
                {
                    var mergePdfName = Path.Combine(folderOutput, string.Format("{0}_{1}.pdf", columbusFilePrefix, fileNo));
                    if (!MergePdfs(folderInput, mergePdfName, ref documentsToMerge)) return false;
                    fileNo++;
                }


                //Run Columbus Task
                Log.Info("Merged pdfs and control files are ready to archive");
                var lstXml = FileProcessHelper.GetFiles(folderOutput, "*.xml");
                var lstPdf = FileProcessHelper.GetFiles(folderOutput, "*.pdf");
                if (!RunColumbus(lstXml, lstPdf)) return false;
                Log.Info("Archive to Columbus Successfully");


                return true;

            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to merge files in {0} to {1}. ex: {3}", folderInput, folderOutput, ex.Message));
                return false;
            }
        }

        #region Welcome Pack
        public static bool WPArchivePDFsToColumbus(string folderInput, string folderOutput, string reportDir, string columbusFilePrefix, List<ColumbusIndex> documentsToMerge, List<ColumbusIndex> lstHodingIdx)
        {
            try
            {
    
                if (!WriteConfirmationFile(lstHodingIdx, reportDir)) return false;

                Log.Info(string.Format("{0} welcome packs are ready to merge.", documentsToMerge.Count()));
                int fileNo = 1;
                while (documentsToMerge.Count != 0)
                {
                    var mergePdfName = Path.Combine(folderOutput, string.Format("{0}_{1}.pdf", columbusFilePrefix, fileNo));
                    if (!MergePdfs(folderInput, mergePdfName, ref documentsToMerge)) return false;
                    fileNo++;
                }
                Log.Info(string.Format("{0} merged pdfs are ready to archive ", fileNo));

                //Run Columbus Task
                var lstXml = FileProcessHelper.GetFiles(folderOutput, "*.xml");
                var lstPdf = FileProcessHelper.GetFiles(folderOutput, "*.pdf");
                if (!RunColumbus(lstXml, lstPdf)) return false;
                Log.Info("Archive to Columbus Sucessful");

                //Use Confimation report to Update Recon_TackingNumber table and send email back to SE
                //Delete archived Pdfs


                return true;

            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to merge files in {0} to {1}. ex: {3}", folderInput, folderOutput, ex.Message));
                return false;
            }
        }

        public static bool WPArchivePDFsToColumbus1(string folderInput, string folderOutput, string reportDir, string columbusFilePrefix, List<ColumbusIndex> lstHodingIdx, List<ColumbusIndex> lstHubnetIdx, ref List<ColumbusIndex> archivedDocs)
        {
            try
            {
                //var idxFiles = FileProcessHelper.GetFiles(folderInput, "*.csv");
                //if (idxFiles.Length != 1)
                //    Log.Fatal("There is no index file or more than 1 index files in " + folderInput);
                //if (idxFiles.Length == 1)
                //{
                //    var indexFile = idxFiles.First();
                //    var csv = new CsvReader(new StringReader(File.ReadAllText(indexFile)));
                //    csv.Configuration.RegisterClassMap<ColumbusHelper.WPColumbusRecordMap>();
                //    csv.Configuration.WillThrowOnMissingField = true;
                //    csv.Configuration.HasHeaderRecord = true;
                //    csv.Configuration.Delimiter = "|";
                //    var records = csv.GetRecords<ColumbusHelper.ColumbusIndex>().ToList();

                ////Enrich pdfs in holding directory
                //var pendingPdfs = FileProcessHelper.GetFiles(folderInput, "*.pdf");
                //var lstHoldingData = new List<WPHoldingData>();

                //foreach (var pdf in pendingPdfs)
                //{
                //    var fileName = new FileInfo(pdf).Name;
                //    var trackingNumber = fileName.Split('_')[1]; //File naming convention: Welcomepack_{Document Number or Tracking number}
                //    var holdingData = new WPHoldingData();
                //    holdingData.fileName = fileName;
                //    holdingData.trackingNumber = trackingNumber;
                //    holdingData.result = "No";
                //    lstHoldingData.Add(holdingData);
                //}


                ////Enrich pdfs in holding directory
                //var pendingPdfs = FileProcessHelper.GetFiles(folderInput, "*.pdf");
                //var lstHoldingData = new List<WPHoldingData>();
                //var documentsToMerge = new List<ColumbusIndex>();
                //foreach (var doc in lstColumbusIdx)
                //{
                //    //var fileName = new FileInfo(pdf).Name;
                //    //var trackingNumber = fileName.Split('_')[1]; //File naming convention: Welcomepack_{Document Number or Tracking number}
                //    var holdingData = new WPHoldingData();
                //    holdingData.fileName = doc.;
                //    holdingData.trackingNumber = trackingNumber;
                //    holdingData.result = "No";
                //    lstHoldingData.Add(holdingData);
                //}

                 var documentsToMerge = new List<ColumbusIndex>();
                 Parallel.ForEach(lstHodingIdx, (currentDoc) =>
                    {
                        foreach (var hubnetData in lstHubnetIdx)
                        {
                            if (hubnetData.DocRefNo == currentDoc.DocRefNo)
                            {
                                currentDoc.IssueDate = hubnetData.IssueDate;
                                currentDoc.AccountNo = hubnetData.AccountNo;
                                currentDoc.CustomerNo = hubnetData.CustomerNo;
                                currentDoc.SalesChannelID = hubnetData.SalesChannelID;
                                currentDoc.FileEnriched = true;
                                documentsToMerge.Add(currentDoc);

                            }
                        }

                    });
                    archivedDocs = documentsToMerge;
                    if (!WriteConfirmationFile(lstHodingIdx, reportDir)) return false;

                    Log.Info(string.Format("{0} welcome packs are ready to merge.", documentsToMerge.Count()));
                    int fileNo = 1;
                    while (documentsToMerge.Count != 0)
                    {
                        var mergePdfName = Path.Combine(folderOutput, string.Format("{0}_{1}.pdf", columbusFilePrefix, fileNo));
                        if (!MergePdfs(folderInput, mergePdfName, ref documentsToMerge)) return false;
                        fileNo++;
                    }
                    Log.Info(string.Format("{0} merged pdfs are ready to archive ", fileNo));

                    //Run Columbus Task
                    var lstXml = FileProcessHelper.GetFiles(folderOutput, "*.xml");
                    var lstPdf = FileProcessHelper.GetFiles(folderOutput, "*.pdf");
                    if (!RunColumbus(lstXml, lstPdf)) return false;
                    Log.Info("Archive to Columbus Sucessful");

                //Use Confimation report to Update Recon_TackingNumber table and send email back to SE
                //Delete archived Pdfs


                return true;

            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to merge files in {0} to {1}. ex: {3}", folderInput, folderOutput, ex.Message));
                return false;
            }
        }

        private static bool WriteConfirmationFile(List<ColumbusIndex> lstHodingIdx, string reportDir)
        {
            try {

                var confirmationFile = Path.Combine(reportDir, string.Format("{0}_WP-Lauch_confirmation.csv", DateTime.Today.ToString("yyyyMMdd")));
                Log.Info(string.Format("Start to write confirmation file: {0}.", confirmationFile));
                var header = "TRACKING_NUMBER,RESULT";
                File.AppendAllText(confirmationFile, header + Environment.NewLine);
                foreach (var record in lstHodingIdx)
                {
                    var result = record.FileEnriched ? "Yes" : "No";
                    File.AppendAllText(confirmationFile, string.Format("{0},{1}", record.DocRefNo, result) + Environment.NewLine);
                }
                Log.Info("Confirmation file is created.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to generate confirmation report. ex: {0}", ex.ToString()));
                return false;
            }
        }
        #endregion


        public static bool MergePdfs(string folderInput, string mergePdfName, ref List<ColumbusIndex> documentsToMerge)
        {

            using (FileStream stream = new FileStream(mergePdfName, FileMode.Create))
            {
                var totalDocs = 0;
                PdfReader reader = null;
                Document document = new Document();
                PdfSmartCopy pdf = new PdfSmartCopy(document, stream);
                List<ColumbusIndex> tempList = new List<ColumbusIndex>();
                try
                {

                    document.Open();
                    foreach (var doc in documentsToMerge)
                    {
                        tempList.Add(doc);
                        if (totalDocs > 0)
                        {
                            if (documentsToMerge[totalDocs - 1].PdfTempData != null)
                                documentsToMerge[totalDocs - 1].PdfTempData = null; //Release from memory
                        }
                        totalDocs++;

                        try
                        {
                            if (doc.PdfTempData == null)
                                doc.PdfTempData = File.ReadAllBytes(Path.Combine(folderInput, doc.FileName));

                            reader = new PdfReader(doc.PdfTempData);
                            doc.Pages = reader.NumberOfPages;

                            if (doc.Pages == 0)
                                throw new Exception("Do not allow documents with zero pages");

                            for (int i = 1; i <= doc.Pages; i++)
                            {
                                //import the page from source pdf
                                var copiedPage = pdf.GetImportedPage(reader, i);
                                // add the page to the new document
                                if (pdf != null)
                                    pdf.AddPage(copiedPage);
                            }

                            if (pdf != null && reader != null)
                            {
                                pdf.FreeReader(reader);
                                pdf.Flush();
                            }

                            if (reader != null)
                                reader.Close();

                            long fileSize = new FileInfo(mergePdfName).Length;
                            long columbusPDFMaxSize = Convert.ToInt64(ConfigurationManager.AppSettings["ColumbusPDFMaxSize"]);
                            if (fileSize > columbusPDFMaxSize) //If file size is bigger than 200mb, start a new file
                            {

                                GenerateControlFile(mergePdfName, tempList);
                                documentsToMerge = documentsToMerge.Except(tempList).ToList();
                                return true;
                            }

                        }
                        catch (Exception ex1)
                        {
                            doc.FileMissing = true;
                            throw new ProcessingException("COMBINE PDF: [" + Path.Combine(folderInput, doc.FileName) + "] " + ex1.Message + " : " + ex1.StackTrace);
                        }

                    }

                    //Generate control file for last merged PDF
                    GenerateControlFile(mergePdfName, tempList);
                    documentsToMerge = documentsToMerge.Except(tempList).ToList();

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format("Failed to merge files in {0} to {1}. ex: {2}", folderInput, mergePdfName, ex));
                    if (reader != null)
                        reader.Close();
                    return false;
                }
                finally
                {
                    if (document != null)
                    {
                        document.Close();
                    }
                }
            }
        }


        public static bool RunColumbus(string[] lstXml, string[] lstPdf)
        {
            try
            {
                var columbusUserName = ConfigurationManager.AppSettings["ColumbusUserName"];
                var columbusPassword = ConfigurationManager.AppSettings["ColumbusPassword"];
                ColumbusServiceReference.ColumbusArchServiceClient columbusService = new ColumbusServiceReference.ColumbusArchServiceClient();
                bool isProduction = Convert.ToBoolean(ConfigurationManager.AppSettings["IsProduction"]);
                return columbusService.RunSimplyEnergyTask(isProduction, columbusUserName, columbusPassword, lstXml, lstPdf);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to archive files to columbus. ex: {0}", ex));
                return false;
            }

        }


        private static void GenerateControlFile(string mergePdfName, List<ColumbusIndex> lstColumbusIdx)
        {
            try
            {

                var typeName = ConfigurationManager.AppSettings["ColumbusSEPDF"];
                //If PDF merged fine, then build the control file XML
                XDocument controlXMl = new XDocument(new XDeclaration("1.0", "", ""));
                XElement eleControl = new XElement("m4cdwxmlarchivecontrolfile");
                controlXMl.Add(eleControl);
                XElement eleInput = new XElement("input");
                eleControl.Add(eleInput);
                XElement eleMulti = new XElement("multidocumentfile");
                eleInput.Add(eleMulti);
                XElement eleFile = new XElement("file");
                eleFile.Value = Path.GetFileName(mergePdfName);
                eleMulti.Add(eleFile);
                XElement eleDocs = new XElement("documents");
                eleMulti.Add(eleDocs);

                foreach (var doc in lstColumbusIdx)
                {
                    if (!doc.FileMissing)
                    {

                        XElement eleDoc = new XElement("document");
                        eleDoc.SetAttributeValue("typename", typeName);
                        XElement eleindexs = new XElement("indexes");
                        eleindexs.Add(GetIndexElement("UID", string.Empty)); //Columbus enriched
                        eleindexs.Add(GetIndexElement("DRN", doc.DocRefNo));
                        eleindexs.Add(GetIndexElement("ACC", doc.AccountNo));
                        eleindexs.Add(GetIndexElement("CRN", doc.CustomerNo));
                        eleindexs.Add(GetIndexElement("PGS", doc.Pages));
                        eleindexs.Add(GetIndexElement("FIR", doc.FirstName));
                        eleindexs.Add(GetIndexElement("LAS", doc.Surname));
                        eleindexs.Add(GetIndexElement("AD1", doc.AddressLine1));
                        eleindexs.Add(GetIndexElement("AD2", doc.AddressLine2));
                        eleindexs.Add(GetIndexElement("AD3", doc.AddressLine3));
                        eleindexs.Add(GetIndexElement("SUB", doc.Suburb));
                        eleindexs.Add(GetIndexElement("STA", doc.State));
                        eleindexs.Add(GetIndexElement("POC", doc.Postcode));
                        eleindexs.Add(GetIndexElement("EMA", doc.EmailAddress));
                        eleindexs.Add(GetIndexElement("DCT", doc.DocType));
                        eleindexs.Add(GetIndexElement("SCI", doc.SalesChannelID));
                        eleindexs.Add(GetIndexElement("ADD", string.Empty)); //Additional Field
                        eleDoc.Add(eleindexs);
                        eleDocs.Add(eleDoc);

                    }
                }

                controlXMl.Save(mergePdfName.Replace(".pdf", ".xml"), SaveOptions.None);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static XElement GetIndexElement(string shortName, string elementValue)
        {
            XElement eleindex = new XElement("index");
            string indexShortName = shortName;
            eleindex.SetAttributeValue("shortname", indexShortName);
            eleindex.Value = elementValue == null ? string.Empty : elementValue;
            return eleindex;
        }

        private static XElement GetIndexElement(string shortName, int? elementValue)
        {
            string indexVal = "";
            if (elementValue.HasValue)
            {
                indexVal = elementValue.ToString();
            }
            XElement eleindex = new XElement("index");
            string indexShortName = shortName;
            eleindex.SetAttributeValue("shortname", indexShortName);
            eleindex.Value = indexVal;
            return eleindex;
        }



    }
}
