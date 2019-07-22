using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class ListImportFaktur
    {
        public int RECNUM { get; set; }
        
        public string UPLOADER { get; set; }
        
        public string LAST_FAKTUR_UPLOADED { get; set; }

        public DateTime UPLOAD_DATETIME { get; set; }

        public DateTime LAST_FAKTUR_UPLOADED_DATETIME { get; set; }

        public string CUST { get; set; }

        public string LOG_FILE { get; set; }
    }
}