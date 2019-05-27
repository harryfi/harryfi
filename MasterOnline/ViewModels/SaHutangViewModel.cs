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

        //add by nurul 15/5/2019
        public List<string> Errors { get; set; } = new List<string>();
    }
}