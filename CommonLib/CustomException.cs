using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using CommonLib;

namespace CommonLib
{
    public class CustomException 
    {

        public class ApplicationErrorException1 : Exception
        {

            public ApplicationErrorException1(String message, Exception ex)
                : base(message, ex)
            {
                String eSubject = dictErrorMessages[EnumErrorMessage.ApplicationErrorSubject];
                string eBody = String.Format("{4} Error{1}{1}Error Message : {0}{1}{1}Error Source : {2}{1}{1}Error StackTrace :{1}{1}{3}", ex.Message, Environment.NewLine, ex.Source, ex.StackTrace, message);
                //Log.Info(String.Format("{0} Error: {1}", methodName, ex.Message));
                SendAdminEmail(eSubject, eBody);
            }
        }


        public enum EnumErrorMessage
        {
            ApplicationErrorSubject,
            InternalProcessingErrorSubject

        }

        public static readonly Dictionary<EnumErrorMessage, String> dictErrorMessages = new Dictionary<EnumErrorMessage, String>
        {
            { EnumErrorMessage.ApplicationErrorSubject, "An Application Error Has Occurred" },
            { EnumErrorMessage.InternalProcessingErrorSubject, "An Internal Error Has Occurred" }
            
        };

        //public static void ApplicationErrorException(String methodName, Exception ex)
        //{
        //    String eSubject = dictErrorMessages[EnumErrorMessage.ApplicationErrorSubject];
        //    string eBody = String.Format("{4} Error{1}{1}Error Message : {0}{1}{1}Error Source : {2}{1}{1}Error StackTrace :{1}{1}{3}", ex.Message, Environment.NewLine, ex.Source, ex.StackTrace, methodName);
        //    Log.Info(String.Format("{0} Error: {1}", methodName, ex.Message));
        //    SendAdminEmail(eSubject, eBody);

        //}


        public static void InternalProcessingException(String eName, String eMessage)
        {
            String eSubject = dictErrorMessages[EnumErrorMessage.InternalProcessingErrorSubject];
            String eBody = String.Format("{0} : {1}", eName, eMessage);
            Log.Info(eBody);
            SendAdminEmail(eSubject, eBody);
        }

        private static void SendAdminEmail(String eSubject, String eBody)
        {
            string smtpHost = ConfigurationManager.AppSettings["InternalSMTPHost"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["EmailPort"]);
            string defaultMailRecipient1 = ConfigurationManager.AppSettings["DefaultMailRecipient1"];
            string defaultMailRecipient2 = ConfigurationManager.AppSettings["DefaultMailRecipient2"];
            string defaultErrorMailFrom = ConfigurationManager.AppSettings["DefaultErrorMailFrom"];

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient client = new SmtpClient();

                mail.To.Add(defaultMailRecipient1);
                mail.To.Add(defaultMailRecipient2);
                mail.From = new MailAddress(defaultErrorMailFrom);

                client.Port = port;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = smtpHost;
                mail.Subject = eSubject;
                mail.Body = eBody;
                client.Send(mail);
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message.ToString());
            }
        }


    }
}
