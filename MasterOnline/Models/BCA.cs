using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class BCA
    {
    }
    public class BCA_Auth : BCAErrorRes
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }

    public class TransferData
    {
        public string CorporateID { get; set; }
        public string SourceAccountNumber { get; set; }
        public string TransactionID { get; set; }
        public string TransactionDate { get; set; }
        public string ReferenceID { get; set; }
        public string CurrencyCode { get; set; }
        public string Amount { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        //public string Remark1 { get; set; }//optional
        //public string Remark2 { get; set; }//optional
    }

    public class TransferResponse : BCAErrorRes
    {
        public string TransactionID { get; set; }
        public string TransactionDate { get; set; }
        public string ReferenceID { get; set; }
        public string Status { get; set; }
    }
    public class BCAErrorRes
    {
        public string ErrorCode { get; set; }
        public Errormessage ErrorMessage { get; set; }
    }

    public class Errormessage
    {
        public string Indonesian { get; set; }
        public string English { get; set; }
    }

    public class BindingBCA : BindingBase
    {
        public string CorporateID { get; set; }
        public string SourceAccountNumber { get; set; }
        public string SourceAccountName { get; set; }
        public string BeneficiaryAccountName { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
    }
}