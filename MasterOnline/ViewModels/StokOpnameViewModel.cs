using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class StokOpnameViewModel
    {
        public STT04A StokOpname { get; set; }
        public List<STT04A> ListStokOpname { get; set; } = new List<STT04A>();
        public STT04B BarangStokOpname { get; set; }
        public List<STT04B> ListBarangStokOpname { get; set; } = new List<STT04B>();
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        public List<STF18> ListGudang { get; set; } = new List<STF18>();
        public List<STT01A> ListBuktiMK { get; set; } = new List<STT01A>();
        public List<string> Errors { get; set; } = new List<string>();
        public string buktiOM { get; set; }
        public string buktiOK { get; set; }
        public string setGD { get; set; }
    }
}