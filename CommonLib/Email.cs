using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class Email
    {

        public static void SendEmail(String eSubject, String eBody)
        {
            string smtpHost = ConfigurationManager.AppSettings["InternalSMTPHost"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["EmailPort"]);
            string defaultMailRecipient1 = ConfigurationManager.AppSettings["DefaultMailRecipient1"];
            string defaultMailRecipient2 = ConfigurationManager.AppSettings["DefaultMailRecipient2"];
            string defaultMailRecipient3 = ConfigurationManager.AppSettings["DefaultMailRecipient3"];
            string defaultNotifyMailFrom = ConfigurationManager.AppSettings["DefaultNotifyMailFrom"];
            var baseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string logFile = String.Format("{0}\\log.txt", baseDir);

            try
            {
          
                MailMessage mail = new MailMessage();
                SmtpClient client = new SmtpClient();

                mail.To.Add(defaultMailRecipient1);
                mail.To.Add(defaultMailRecipient2);
                mail.To.Add(defaultMailRecipient3);
                mail.From = new MailAddress(defaultNotifyMailFrom);
                  
                client.Port = port;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = smtpHost;
                mail.Subject = eSubject;
                mail.Body = String.Format("{0}{1}{1}Please refer log file for details:{1}{2}",eBody,Environment.NewLine,logFile);
                client.Send(mail);
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
            }
        }

        public static void SendEmail(String eSubject, String eBody, string toEmailAddr)
        {
            string smtpHost = ConfigurationManager.AppSettings["InternalSMTPHost"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["EmailPort"]);
            string defaultNotifyMailFrom = ConfigurationManager.AppSettings["DefaultNotifyMailFrom"];
            var baseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string logFile = baseDir != null ? String.Format("{0}\\log.txt", baseDir) : "";

            try
            {

                MailMessage mail = new MailMessage();
                SmtpClient client = new SmtpClient();

                mail.To.Add(toEmailAddr);
          
                mail.From = new MailAddress(defaultNotifyMailFrom);

                client.Port = port;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = smtpHost;
                mail.Subject = eSubject;
                mail.Body = String.Format("{0}{1}{1}Please refer log file for details:{1}{2}", eBody, Environment.NewLine, logFile);
                client.Send(mail);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        #region send outbound email
        public class EmailPara
        {
            public string eSubject { get; set; }
            public string eBody { get; set; }
            public List<string> mailRecipients { get; set; }
            public List<string> ccRecipients { get; set; }
            public List<string> attachments { get; set; }
        }

        public static bool SendOutboundEmail(EmailPara emailPara)
        {
            string smtpHost = ConfigurationManager.AppSettings["InternalSMTPHost"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["EmailPort"]);
            string defaultNotifyMailFrom = ConfigurationManager.AppSettings["DefaultNotifyMailFrom"];

            try
            {

                MailMessage mail = new MailMessage();
                SmtpClient client = new SmtpClient();

                if (emailPara.mailRecipients != null)
                    if (emailPara.mailRecipients.Any())
                    {
                        foreach (var mailRecipient in emailPara.mailRecipients)
                        {
                            mail.To.Add(mailRecipient);
                        }
                    }
                    else
                    {
                        Log.Info("No Mail Recipient");
                        return false;
                    }

                if(emailPara.ccRecipients != null)
                    if (emailPara.ccRecipients.Any())
                    {
                        foreach (var ccRecipient in emailPara.ccRecipients)
                        {
                            mail.CC.Add(ccRecipient);
                        }
                    }

             
                mail.Bcc.Add(ConfigurationManager.AppSettings["DefaultMailRecipient3"]);
                mail.From = new MailAddress(defaultNotifyMailFrom);

                client.Port = port;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = smtpHost;
                mail.Subject = emailPara.eSubject;
                mail.Body = emailPara.eBody;
                if (emailPara.attachments!= null)                                                        
                {
                    foreach (var attachedFile in emailPara.attachments)
                    {
                        Attachment attachment = new System.Net.Mail.Attachment(attachedFile);
                        mail.Attachments.Add(attachment);
                    }
                }

                client.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error sending email. Ex: {0}",ex.Message));
                return false;
            }
        }


        #endregion
    }
}
