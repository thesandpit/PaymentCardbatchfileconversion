using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Diagnostics;
using System.Configuration;

namespace CommonLib
{
    public class HelperClass
    {
       
        public enum ValidationType
        {
            Email, Date, Currency, RealNumber, PositiveInt, IntegerNonZero, DecimalNonZero
        }
        private Hashtable RegexLib
        {
            get
            {
                Hashtable RegexLib = new Hashtable();

                //RegexLib.Add(ValidationType.Email, @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$");
                RegexLib.Add(ValidationType.Email, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
                RegexLib.Add(ValidationType.Date, @"");
                RegexLib.Add(ValidationType.Currency, @"^\d+(?:\.\d{0,2})?$");
                RegexLib.Add(ValidationType.RealNumber, @"^[-+]?\d+(\.\d+)?$");
                RegexLib.Add(ValidationType.PositiveInt, @"^\d+$");
                RegexLib.Add(ValidationType.IntegerNonZero, @"^[1-9]+[0-9]*$");
                RegexLib.Add(ValidationType.DecimalNonZero, @"^[1-9]+[0-9]*(?:\.\\d+)?$");

                return RegexLib;
            }
        }

        public String GetValidationExpression(ValidationType type)
        {
            String strExpression = "";
            strExpression = RegexLib[type].ToString();
            return strExpression;
        }

        public Int64 parseInt64(int? value)
        {
            Int64 retVal = 0;
            if (value.HasValue)
            {
                retVal = (Int64)value;
            }
            return retVal;
        }
        public Int64 parseInt64(string value)
        {
            Int64 retVal = 0;
            Int64.TryParse(value, out retVal);
            return retVal;
        }
        public Int64 parseInt64(long? value)
        {
            Int64 retVal = 0;
            if (value.HasValue)
            {
                retVal = (Int64)value;
            }
            return retVal;
        }
        public int? parseInt(int value)
        {
            int? retVal = null;
            retVal = value;
            return retVal;
        }
        public int? parseInt(string value)
        {
            Int64 tempVal = 0;
            Int64.TryParse(value, out tempVal);
            int? retVal = null;
            retVal = (int?)tempVal;
            return retVal;
        }
        public DateTime parseDateTime(DateTime value)
        {
            DateTime dateOutputVal = DateTime.MinValue;
            if (value != null)
                dateOutputVal = value;

            return dateOutputVal;
        }

        public int parseInt(int? value)
        {
            int retVal = 0;
            if (value.HasValue)
            {
                retVal = value.Value;
            }
            return retVal;
        }
        public Int32 parseInt32(string value)
        {
            Int32 retVal = 0;
            Int32.TryParse(value, out retVal);
            return retVal;
        }

        public double parseDouble(double? value)
        {
            double retVal = 0.0;
            if (value.HasValue)
            {
                retVal = (double)value;
            }
            return retVal;
        }
        public double parseDouble(decimal? value)
        {
            double retVal = 0.0;
            if (value.HasValue)
            {
                retVal = (double)value;
            }
            return retVal;
        }
        public Double parseDouble(string value)
        {
            Double retVal = 0;
            Double.TryParse(value, out retVal);
            return retVal;
        }

        public bool parseBool(bool? value)
        {
            bool retVal = false;
            if (value.HasValue)
            {
                retVal = (bool)value;
            }
            return retVal;
        }
        public bool parseBool(string value)
        {
            bool retVal = false;
            if (value == "1" || value == "True" || value == "true")
            {
                retVal = true;
            }
            return retVal;
        }
        public String parseString(int? value)
        {
            String retVal = "";
            if (value.HasValue)
            {
                retVal = value.ToString();
            }
            return retVal;
        }
        public String parseString(decimal? value)
        {
            String retVal = "";
            if (value.HasValue)
            {
                retVal = value.ToString();
            }
            return retVal;
        }
        public String parseString(double? value)
        {
            String retVal = "";
            if (value.HasValue)
            {
                retVal = value.ToString();
            }
            return retVal;
        }
        public String parseString(bool? value)
        {
            String retVal = "";
            if (value.HasValue)
            {
                retVal = value.ToString();
            }
            return retVal;
        }
        public String parseString(string value)
        {
            String retVal = "";
            if (!String.IsNullOrEmpty(value))
            {
                retVal = value.ToString();
            }
            return retVal;
        }

        public byte[] GetEmbeddedResource(string fullAssemblyNameSpace)
        {
            Assembly objCurrentAssembly = Assembly.GetCallingAssembly();
            Stream objStream = objCurrentAssembly.GetManifestResourceStream(fullAssemblyNameSpace);
            byte[] arrResourceBytes = new byte[objStream.Length];

            objStream.Read(arrResourceBytes, 0, (int)objStream.Length);

            return arrResourceBytes;
        }

        public void WriteToFile(string path, string message, bool append)
        {
            if (append)
            {
                File.AppendAllText(path, message);
            }
            else
            {
                File.WriteAllText(path, message);
            }

        }

        public T EnumFromString<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static String EnumToString(Enum eff)
        {
            return Enum.GetName(eff.GetType(), eff);
        }
        
        public String BuildErrorEmailStrings(string filename, string tempString, string emailBody, /*StatusCode objStatusCode,*/ ref string errorDataFileReporting)
        {
            emailBody += tempString + "\n";
            errorDataFileReporting += String.Format("1,{0}," + "\"" + tempString + "\"\n"/*, (int)objStatusCode*/);

            return emailBody;
        }

    }


}
