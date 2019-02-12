using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class PromptCustomerViewModel
    {
        public string KODE { get; set; }
        public string NAMA { get; set; }
        public string MARKETPLACE { get; set; }
        public string IDMARKET { get; set; }
    }

    public class PromptBarangViewModel
    {
        public string KODE { get; set; }
        public string NAMA { get; set; }
        public Double HARGA { get; set; }

    }

    public class PromptBrg
    {
        public string NAMA_BRG { get; set; }
        public List<PromptBarangViewModel> data { get; set; }
        public string typeBrg { get; set; }
    }
}