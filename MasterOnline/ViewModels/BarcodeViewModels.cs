using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;


namespace MasterOnline.ViewModels
{
    public class BarcodeViewModels
    {
    }

    public class ScanBarcodePickingBarang
    {
        public string brg { get; set; }
        public string code { get; set; }
        public int qty { get; set; }
        public int input_qty { get; set; }
        public bool isValid { get; set; }
    }
    public class ScanBarcodePickingBarangViewModel
    {
        public string NO_PL { get; set; }
        public int jmlBrg { get; set; }
        public int maxBrg { get; set; }
        public int jmlQty { get; set; }
        public int maxQty { get; set; }
        public List<ScanBarcodePickingBarang> dataScan { get; set; }
        public string currentScan { get; set; }
    }
}