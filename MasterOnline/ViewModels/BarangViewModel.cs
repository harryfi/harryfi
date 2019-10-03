using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class TableMenuBarang1PartialViewModel
    {
        public string BRG { get; set; }

        public string NAMA { get; set; }

        public string NAMA2 { get; set; }

        public string KET_SORT1 { get; set; }

        public string KET_SORT2 { get; set; }

        public int? ID { get; set; }

        public double HJUAL { get; set; }

        public string LINK_GAMBAR_1 { get; set; }

        public double MIN { get; set; }

        public double QOH { get; set; }

        public double QOO { get; set; }

        public double QtySales { get; set; }

        //ADD BY NURUL 23/9/2019
        public double SELISIH { get; set; }
        public string JENIS { get; set; }        
        //END ADD BY NURUL 23/9/2019

    }

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
        public List<QOH_QOO_ALL_ITEM> Stok { get; set; }
        public string errorHargaPerMP { get; set; }
        //add by nurul 31/1/2019
        public string BRG { get; set; }
        public string MULAI { get; set; }
        public string AKHIR { get; set; }
        public int MARKET { get; set; }
    }
    
}