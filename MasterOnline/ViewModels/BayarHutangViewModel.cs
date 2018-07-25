using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class BayarHutangViewModel
    {
        public APT03A Hutang { get; set; }
        public List<APT03A> ListHutang { get; set; } = new List<APT03A>();
        public APT03B HutangDetail { get; set; }
        public List<APT03B> ListHutangDetail { get; set; } = new List<APT03B>();
        public List<SIT01A> ListFaktur { get; set; } = new List<SIT01A>();
        public List<APT01D> ListSisa { get; set; } = new List<APT01D>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}