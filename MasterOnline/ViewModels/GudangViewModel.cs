using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class GudangViewModel
    {
        public STF18 Gudang { get; set; }
        public List<STF18> ListGudang { get; set; } = new List<STF18>();
        public List<string> Errors { get; set; } = new List<string>();
        
        //add by nurul 5/11/2020
        public bool gd_default { get; set; }
        //end add by nurul 5/11/2020
    }
}