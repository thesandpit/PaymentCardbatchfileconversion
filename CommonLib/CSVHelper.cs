using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class CSVHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">csv path</param>
        /// <param name="map">e.g. new ClassLib.ConfirmationCSVMap()</param>
        /// <param name="type">e.g. typeof(ClassLib.ConfirmationCSV)</param>
        /// <returns></returns>
        public List<object> ReadCSVToList(string filePath, CsvClassMap map, Type type, CsvConfiguration csvConfig)
        {
            try
            {

                StreamReader file = new StreamReader(filePath);
                using (CsvReader objCsvReader = new CsvReader(file))
                {
                    objCsvReader.Configuration.RegisterClassMap(map);
                    objCsvReader.Configuration.Delimiter = csvConfig.Delimiter;
                    objCsvReader.Configuration.TrimFields = true;
                    objCsvReader.Configuration.WillThrowOnMissingField = true;
                    objCsvReader.Configuration.HasHeaderRecord = csvConfig.HasHeaderRecord; //default is true
    
                    return objCsvReader.GetRecords(type).ToList();
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Fail to Build CSV List from CSV File" + ex.Message);
            }
        }


        /// <summary>
        /// WriteListToCSV
        /// </summary>
        /// <param name="destPath"></param>
        /// <param name="list">e.g. hubnetFileErrorList.Cast<object>().ToList()</param>
        /// <param name="csvConfig">e.g. new CsvConfiguration { }, Default Delimiter is ,</param>
        public void WriteListToCSV(string destPath, List<object> list, CsvConfiguration csvConfig)
        {
            try
            {
                TextWriter writer = new StreamWriter(destPath);
                CsvConfiguration objConfig = new CsvConfiguration
                {
                    Delimiter = csvConfig.Delimiter,
                    HasHeaderRecord = true,
                    QuoteAllFields = csvConfig.QuoteAllFields
                };

                using (CsvWriter csvWriter = new CsvWriter(writer, objConfig))
                {
                    csvWriter.WriteRecords(list);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fail to Write List to CSV" + ex.Message);
            }
        }

    }
}
