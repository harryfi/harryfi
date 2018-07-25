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
    }
}