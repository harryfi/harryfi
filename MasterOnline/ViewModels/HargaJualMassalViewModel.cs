using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class HargaJualMassalViewModel
    {
        public string NO_BUKTI { get; set; }
        public string CUST { get; set; }
        public string FILE_1 { get; set; }
        public int JML_BRG_1 { get; set; }
        public string FILE_2 { get; set; }
        public int JML_BRG_2 { get; set; }
        public string FILE_3 { get; set; }
        public int JML_BRG_3 { get; set; }
        public string FILE_4 { get; set; }
        public int JML_BRG_4 { get; set; }
        public DateTime TGL_PROSES { get; set; }
        public int JAM_PROSES { get; set; }
        public DateTime TGL_INPUT { get; set; }
        public int STATUS { get; set; }
        public string USERNAME { get; set; }

    }
}