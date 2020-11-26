using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models.Api
{
    public class JsonData
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string SelDate { get; set; }
        public string StatusTransaksi { get; set; }
        public int? RecNumPesanan { get; set; }
        public string PassLama { get; set; }
        public string PassBaru { get; set; }
        public string SearchParam { get; set; }
        public int SortBy { get; set; }
        public string DbPath { get; set; }
        public long AccId { get; set; }
        public string Username { get; set; }
        
        // Add by Fauzi for API Update Stok perbarang 26 November 2020
        public string brg { get; set; }
        // End 26 November 2020
    }
}