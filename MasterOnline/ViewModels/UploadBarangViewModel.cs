using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class UploadBarangViewModel
    {
        public List<STF02E> ListKategoriMerk { get; set; } = new List<STF02E>();
        public List<STF02E> ListKategoriBrg { get; set; } = new List<STF02E>();
        public List<ARF01> ListMarket { get; set; } = new List<ARF01>();
        public String Username { get; set; }
        public STF02 Stf02 { get; set; }
        public List<TEMP_BRG_MP> ListTempBrg { get; set; } = new List<TEMP_BRG_MP>();
        public TEMP_BRG_MP TempBrg { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public int failedRecord { get; set; }
        public string contRecursive { get; set; }
        //public int haveVarian { get; set; }
        public int tipeBarang { get; set; }

    }

    public class SyncBarangViewModel : UploadBarangViewModel
    {
        public bool Recursive { get; set; }
        public int Page { get; set; }
        public int RecordCount { get; set; }
        public int BLProductActive { get; set; }
        public int exception { get; set; }
        public int totalData { get; set; }//add 18 Juli 2019, show total record
    }

    public class SimpleJsonObject
    {
        public int Total { get; set; }
        public string Errors { get; set; }
    }
}