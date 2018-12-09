using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;
using DataAccessLayer;


namespace CommonLib
{
    public class ProcessingException: Exception
    {
        public ProcessingException(string message)
            : base(message)
        {
            Log.Error(message);
            Email.SendEmail(string.Format("Simply {0} Processing Error", VarClass.jobTypeDesc), message);
        }

        public ProcessingException(string message, string docDesc)
            : base(message)
        {
            Log.Error(message);
            Email.SendEmail(string.Format("Simply {0} Processing Error", docDesc), message);
        }

        public ProcessingException(string message, Exception inner, string docDesc)
            : base(message, inner)
        {
            Log.Info(inner.ToString());
            Email.SendEmail(string.Format("Simply {0} Processing Error",docDesc), message);
           
        }
    }
}
