using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class KategoriBarangViewModel
    {
        public STF02E Kategori { get; set; }
        public List<STF02E> ListKategori { get; set; } = new List<STF02E>();
        //add by nurul 2/7/2019
        public List<string> Errors { get; set; } = new List<string>();
        //end add by nurul 2/7/2019
    }
}