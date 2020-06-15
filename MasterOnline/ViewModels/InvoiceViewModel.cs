using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class InvoiceViewModel
    {
        public PBT01A Invoice { get; set; }
        public List<Subscription> ListSubs { get; set; } = new List<Subscription>();
        public List<PBT01A> ListInvoice { get; set; } = new List<PBT01A>();
        public PBT01B InvoiceDetail { get; set; }
        public List<PBT01B> ListInvoiceDetail { get; set; } = new List<PBT01B>();
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        public List<ARF01C> ListPembeli { get; set; } = new List<ARF01C>();
        public List<ARF01> ListPelanggan { get; set; } = new List<ARF01>();
        public List<Marketplace> ListMarketplace { get; set; } = new List<Marketplace>();
        public List<APT03B> ListNInvoice { get; set; } = new List<APT03B>();
        public List<String> Errors { get; set; } = new List<string>();

        //add by nurul 9/6/2020
        public string setGd { get; set; }
        public string noInv { get; set; }
        public string noCust { get; set; }
        public List<STF18> listGudang { get; set; }
        public List<brgForReturInvPrompt> listBarangReturInv = new List<brgForReturInvPrompt>();
        //end add by nurul 9/6/2020
    }

    //add by nurul 9/6/2020
    public class FakturForReturInvPrompt
    {
        public string inv { get; set; }
        public DateTime tgl { get; set; }
        public string nama { get; set; }
        public double netto { get; set; }
    }
    public class brgForReturInvPrompt
    {
        public int recnum { get; set; }
        public string brg { get; set; }
        public string nama { get; set; }
        public double qty { get; set; }
        public string gudang { get; set; }
    }
    //end add by nurul 9/6/2020
}