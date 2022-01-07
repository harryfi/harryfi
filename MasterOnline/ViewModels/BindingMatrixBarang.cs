﻿using System;
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
    public class BindingUnlinkBarang
    {
        public List<DataUnlinkBarang> barang { get; set; }
    }
    public class DataUnlinkBarang
    {
        public string BRG { get; set; }
        public string NAMABRG { get; set; }
        public string BRG_MP { get; set; }
        public string LINK { get; set; }
        public string NAMAMARKET { get; set; }
        public string PERSO { get; set; }
        public int IDMARKET { get; set; }
    }
    public class DataHistoryUnlinkBarang
    {
        public string BRG { get; set; }
        public string NAMABRG { get; set; }
        public string NAMAMARKET { get; set; }
        public string PERSO { get; set; }
        public int IDMARKET { get; set; }
        public DateTime TGL { get; set; }
        public string USERNAME { get; set; }
    }

    public class BindingListCustomer
    {
        public List<DataListCustomer> customer { get; set; }
    }
    public class DataListCustomer
    {
        public string CUST { get; set; }
        public string NAMAMARKET { get; set; }
        public string PERSO { get; set; }
        public int RECNUM { get; set; }
    }
}