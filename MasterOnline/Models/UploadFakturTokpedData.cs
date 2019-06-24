using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{

    public class UploadFakturTokpedData
    {
        public UploadFakturTokpedDataDetail[] data { get; set; }
    }

    public class UploadFakturTokpedDataDetail
    {
        public object this[string propertyName]
        {
            get
            {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(UploadFakturTokpedDataDetail);
                System.Reflection.PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(UploadFakturTokpedDataDetail);
                System.Reflection.PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);

            }
        }
        public string Count { get; set; }
        public string Invoice { get; set; }
        public string PaymentDate { get; set; }
        public string OrderStatus { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string Quantity { get; set; }
        public string StockKeepingUnitSKU { get; set; }
        public string Notes { get; set; }
        public string PriceRp { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string Recipient { get; set; }
        public string RecipientNumber { get; set; }
        public string RecipientAddress { get; set; }
        public string Courier { get; set; }
        public string ShippingPricefeeRp { get; set; }
        public string InsuranceRp { get; set; }
        public string TotalShippingFeeRp { get; set; }
        public string TotalAmountRp { get; set; }
        public string AWB { get; set; }
        public string JenisLayanan { get; set; }
    }
}