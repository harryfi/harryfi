using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class SaPiutangViewModel
    {
        public ART01A Piutang { get; set; }
        public List<ART01A> ListPiutang { get; set; } = new List<ART01A>();

        //add by nurul 15/5/2019
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class multiSKUViewModel
    {
        public STF03C multiSKU { get; set; }
        public List<STF03C> listMultiSKU { get; set; } = new List<STF03C>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<STF02> listDetailSKU { get; set; } = new List<STF02>();
        public string Brg_Acuan { get; set; }
        public string statusAddonMultiSKU { get; set; }
    }

    //add by nurul 5/10/2020
    public class BundlingViewModel
    {
        public STF03 Bundling { get; set; }
        public List<STF03> listBundling { get; set; } = new List<STF03>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<STF02> listDetailBundling { get; set; } = new List<STF02>();
        public string Brg_Bundling { get; set; }
        public double Qty_Bundling { get; set; }
    }
    //end add by nurul 5/10/2020
}