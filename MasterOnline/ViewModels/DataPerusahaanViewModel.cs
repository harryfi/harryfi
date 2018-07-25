using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class DataPerusahaanViewModel
    {
        public SIFSY DataUsaha { get; set; } = new SIFSY();
        public SIFSYS_TAMBAHAN DataUsahaTambahan { get; set; } = new SIFSYS_TAMBAHAN();
        public List<String> Errors { get; set; } = new List<string>();
    }
}