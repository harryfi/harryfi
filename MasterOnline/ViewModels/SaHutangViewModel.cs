using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class SaHutangViewModel
    {
        public APT01A Hutang { get; set; }
        public List<APT01A> ListHutang { get; set; } = new List<APT01A>();
    }
}