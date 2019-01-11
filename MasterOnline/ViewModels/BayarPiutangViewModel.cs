using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class BayarPiutangViewModel
    {
        public ART03A Piutang { get; set; }
        public List<ART03A> ListPiutang { get; set; } = new List<ART03A>();
        public ART03B PiutangDetail { get; set; }
        public List<ART03B> ListPiutangDetail { get; set; } = new List<ART03B>();
        public List<SIT01A> ListFaktur { get; set; } = new List<SIT01A>();
        public List<ART01D> ListSisa { get; set; } = new List<ART01D>();
        public List<string> Errors { get; set; } = new List<string>();
        //add by Tri 4 Jan 2019, autoload faktur
        public double bayarPiutang { get; set; }
    }
}