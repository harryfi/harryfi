﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class FakturJson
    {
        public int? RecNum { get; set; }
        public string NO_BUKTI { get; set; }

        //Piutang
        public double? Sisa { get; set; }

    }

    public class refJson
    {
        public string NO_BUKTI { get; set; }
        //add by nurul 23/10/2019
        public string noRef { get; set; }
        public string tglRef { get; set; }
    }
    //end add by nurul 14/11/2019
}