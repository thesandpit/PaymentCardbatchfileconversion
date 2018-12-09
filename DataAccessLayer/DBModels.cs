using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class DBModels
    {
        public class ReconDataReceivedModel
        {

            public string DR_UID { get; set; }
            public int DR_ClientID { get; set; }
            public int DR_JobID { get; set; }
            public string DR_Datetime { get; set; }
            public int DR_TotalReceived { get; set; }
            public string DR_DocType { get; set; }
            public string DR_FileName { get; set; }
            public string DR_DPUID { get; set; }
            public string DocTypeDesc { get; set; }

        }

        public class ReconDataProcessedModel
        {
            public string DP_FileName { get; set; }
            public DateTime DP_DateProcessed { get; set; }
            public int DP_TotalProcessed { get; set; }
            public string DP_ArchSets { get; set; }
            public string DocTypeDesc { get; set; }
        }


        public class ReconTrackingModel
        {
            public int DR_UID { get; set; }
            public string TrackingNo { get; set; }
            public int Flag { get; set; }
            public string FileName { get; set; }
        }

    }
}
