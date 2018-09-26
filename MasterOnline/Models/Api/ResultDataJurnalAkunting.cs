using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models.Api
{
    public class ResultDataJurnalAkunting
    {
        public GLFTRAN1 Jurnal { get; set; }
        public double Debet { get; set; }
        public double Kredit { get; set; }
    }
}