using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class BarangViewModel
    {
        public STF02 Stf02 { get; set; }
        public IList<STF02H> ListHargaJualPermarket { get; set; }
        public IList<STF02H> ListHargaJualPermarketView { get; set; } = new List<STF02H>();
        public List<STF02> ListStf02S { get; set; } = new List<STF02>();
        public List<STF02E> ListKategoriMerk { get; set; } = new List<STF02E>();
        public IList<ARF01> ListMarket { get; set; } = new List<ARF01>();
        public SIFSY DataUsaha { get; set; }
        public String Username { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<CATEGORY_BLIBLI> ListCategoryBlibli { get; set; }
        public List<API_LOG_MARKETPLACE_PER_ITEM> StatusLog { get; set; }
        public string errorHargaPerMP { get; set; }
    }
}