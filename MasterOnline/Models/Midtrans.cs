using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class Midtrans
    {
    }
    

    public class MidtransTransactionData
    {
        public string status_code { get; set; }
        public string status_message { get; set; }
        public string transaction_id { get; set; }
        public string masked_card { get; set; }
        public string order_id { get; set; }
        public string payment_type { get; set; }
        public string transaction_time { get; set; }
        public string transaction_status { get; set; }
        public string fraud_status { get; set; }
        public string bank { get; set; }
        public string gross_amount { get; set; }
        public string signature_key { get; set; }
        public string saved_token_id { get; set; }
        public string saved_token_id_expired_at { get; set; }
    }
    public class BindReqSnap
    {
        public TransactionDetail transaction_details { get; set; }
        public CreditCard credit_card { get; set; }
        public CustomerDetail customer_details { get; set; }
        public string user_id { get; set; }
        //public List<string> enabled_payments { get; set; }
    }
    public class AutoDebetCC
    {
        public string payment_type { get; set; }
        public TransactionDetail transaction_details { get; set; }
        public AutoCC credit_card { get; set; }
    }
    public class AutoCC
    {
        public string token_id { get; set; }
    }
    public class CreditCard
    {
        public bool secure { get; set; }
        public bool save_card { get; set; }
        public bool save_token_id { get; set; }
    }
    public class TransactionDetail
    {
        public string order_id { get; set; }
        public long gross_amount { get; set; }
    }
    public class CustomerDetail
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        //public string email { get; set; }
        public string phone { get; set; }
        //public Address billing_address { get; set; }
        //public Address shipping_address { get; set; }
    }
    public class BindResSnap
    {
        public string token { get; set; }
        public string error_messages { get; set; }
    }
}