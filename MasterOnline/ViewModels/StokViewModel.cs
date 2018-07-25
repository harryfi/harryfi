﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class StokViewModel
    {
        public STT01A Stok { get; set; }
        public List<STT01A> ListStok { get; set; } = new List<STT01A>();
        public STT01B BarangStok { get; set; }
        public List<STT01B> ListBarangStok { get; set; } = new List<STT01B>();
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        public List<STF18> ListGudang { get; set; } = new List<STF18>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}