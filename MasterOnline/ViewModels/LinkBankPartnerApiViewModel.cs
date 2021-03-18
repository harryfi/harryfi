namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public class LinkBankPartnerApiViewModel
    {
        public List<LinkBankPartnerApiVIEW> ListBankApi { get; set; } = new List<LinkBankPartnerApiVIEW>();
        public LinkBankPartnerApiVIEW BankApi { get; set; } = new LinkBankPartnerApiVIEW();
    }

    public partial class LinkBankPartnerApiVIEW
    {
        public string NAMA { get; set; }
        public string CUST { get; set; }
        public string KODE_BANK { get; set; }
        public string BRANCH_NAME { get; set; }
        public string PAID { get; set; }

    }


}