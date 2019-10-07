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
    
    public class mdlPartner
    {
        public Int64 PartnerId { get; set; }
        public String Email { get; set; }
        public String Username { get; set; }
        public String NoHp { get; set; }
        public String PhotoKtpUrl { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public Boolean Status { get; set; }
        public Boolean StatusSetuju { get; set; }
        public string PhotoKtpBase64 { get; set; }
        public int TipePartner { get; set; }
        public string NamaTipe { get; set; }
        public string KodeRefPilihan { get; set; }
        public double komisi_support { get; set; }
        public double komisi_subscribe { get; set; }
        public double komisi_subscribe_gold { get; set; }
        public DateTime? TGL_DAFTAR { get; set; }
    }
}
