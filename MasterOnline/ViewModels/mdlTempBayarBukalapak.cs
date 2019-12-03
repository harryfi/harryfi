using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Web;
    using MasterOnline.Models;
    
    public class mdlTempBayarBukalapak
    {
        //public DateTime TransactionDate { get; set; }
        //public string TransactionType { get; set; }
        //public string FeeName { get; set; }
        //public string TransactionNumber { get; set; }
        //public string Details { get; set; }
        //public string SellerSKU { get; set; }
        //public string LazadaSKU { get; set; }
        //public string Amount { get; set; }
        //public string VATinAmount { get; set; }
        //public string WHTAmount { get; set; }
        //public string WHTincludedinAmount { get; set; }
        //public string Statement { get; set; }
        //public string PaidStatus { get; set; }
        //public string OrderNo { get; set; }
        //public string OrderItemNo { get; set; }
        //public string OrderItemStatus { get; set; }
        //public string ShippingProvider { get; set; }
        //public string ShippingSpeed { get; set; }
        //public string ShipmentType { get; set; }
        //public string Reference { get; set; }
        //public string Comment { get; set; }
        //public string PaymentRefId { get; set; }

        public DateTime? Waktu { get; set; }
        public double Mutasi { get; set; }
        public double Saldo { get; set; }
        public string Note { get; set; }
        public string Keterangan { get; set; }
        public string Bukti { get; set; }
    }

    public class mdlTempCSVBayarBukalapak
    {
        public string Waktu { get; set; }
        public string Mutasi { get; set; }
        public string Saldo { get; set; }
        public string Keterangan { get; set; }
    }
}
