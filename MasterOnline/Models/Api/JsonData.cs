﻿using System;
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
    }
}