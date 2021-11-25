using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class BindingMatrixBarang
    {
        public List<DataMatrixBarang> barang { get; set; }
        public List<ARF01> akun { get; set; }
    }
    public class DataMatrixBarang
    {
        public string BRG { get; set; }
        public string NAMABRG { get; set; }
        public string BRG_MP { get; set; }
        public string LINK { get; set; }
        public int IDMARKET { get; set; }
        public int RECNUM { get; set; }
    }
}