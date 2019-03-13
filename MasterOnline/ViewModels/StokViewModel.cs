using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class StokViewModel
    {
        public STT01A Stok { get; set; }
        public List<STT01A> ListStok { get; set; } = new List<STT01A>();
        public STT01B BarangStok { get; set; }
        public List<STT01B> ListBarangStok { get; set; } = new List<STT01B>();
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        public List<STF18> ListGudang { get; set; } = new List<STF18>();
        public List<string> Errors { get; set; } = new List<string>();
        //add by nurul 8/3/2019 set default gudang dr sifsys
        public List<QOH_PER_GD> ListQOHPerGD { get; set; } = new List<QOH_PER_GD>();
        public string setGd { get; set; }
        //end add by nurul 8/3/2019 set default gudang dr sifsys
    }
}