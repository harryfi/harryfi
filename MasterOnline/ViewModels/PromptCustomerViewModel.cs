using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class PromptAccountViewModel
    {
        public string era_db_path { get; set; }
        public string data_source_path { get; set; }
        public string email { get; set; }
        public string nama { get; set; }
        public string namatoko { get; set; }
    }
    
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

        public string cust { get; set; }
    }

    //ADD BY NURUL 19/6/2019
    public class PromptBrgBaru
    {
        public string NAMA_BRG { get; set; }
        public string typeBrg { get; set; }

        public string KODE { get; set; }
        public string NAMA { get; set; }
        public Double HARGA { get; set; }
        //public string ulang { get; set; }
    }
    //END ADD BY NURUL 19/6/2019
}