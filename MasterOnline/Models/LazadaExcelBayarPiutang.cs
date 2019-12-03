using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class LazadaExcelBayarPiutang
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string FeeName { get; set; }
        public string TransactionNumber { get; set; }
        public string Details { get; set; }
        public string SellerSKU { get; set; }
        public string LazadaSKU { get; set; }
        public double Amount { get; set; }
        public double VATinAmount { get; set; }
        public double WHTAmount { get; set; }
        public string WHTincludedinAmount { get; set; }
        public string Statement { get; set; }
        public string PaidStatus { get; set; }
        public string OrderNo { get; set; }
        public string OrderItemNo { get; set; }
        public string OrderItemStatus { get; set; }
        public string ShippingProvider { get; set; }
        public string ShippingSpeed { get; set; }
        public string ShipmentType { get; set; }
        public string Reference { get; set; }
        public string Comment { get; set; }
        public string PaymentRefId { get; set; }
    }
}