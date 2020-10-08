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
        public string rak { get; set; }
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
        public List<string> listDelete { get; set; }
    }

    public class ScanBarcodePackingPesananViewModel
    {
        public string NO_PL { get; set; }
        public string nobuk { get; set; }
        public int jmlOrder { get; set; }
        public int maxOrder { get; set; }
        public List<ScanBarcodePackingPesanan> dataScan { get; set; }
    }

    public class ScanBarcodePackingPesanan
    {
        public string brg { get; set; }
        public string nama { get; set; }
        public string code { get; set; }
        public string rak { get; set; }
        public int qty { get; set; }
        public bool isValid { get; set; }
    }

    public class BarcodedanRakModel
    {
        public List<DataBarcodeVarian> listBrg { get; set; }
    }

    public class DataBarcodeVarian
    {
        public string barcode { get; set; }
        public string rak { get; set; }
        public int id { get; set; }

    }
}