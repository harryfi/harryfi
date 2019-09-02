using MasterOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class PackingListViewModel
    {
        public List<SOT03A> listParent { get; set; }
        public SOT03A packingList { get; set; }
        public SOT03B detailPackingList { get; set; }
        public List<SOT03B> listDetailPacking { get; set; }
        public List<SOT03B> listPesanan { get; set; }
    }
}