using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class BuyerViewModel
    {
        public ARF01C Pembeli { get; set; }
        public List<ARF01C> ListPembeli { get; set; } = new List<ARF01C>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}