﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class MasterStrukturVarViewModel
    {
        public STF02E Kategori { get; set; }
        public STF20 Variant_Level_1 { get; set; } 
        public STF20 Variant_Level_2 { get; set; } 
        public STF20 Variant_Level_3 { get; set; } 
        public STF20B VariantOpt_Level_1 { get; set; } 
        public STF20B VariantOpt_Level_2 { get; set; } 
        public STF20B VariantOpt_Level_3 { get; set; } 
        public List<STF20B> VariantOptInDb { get; set; } = new List<STF20B>();
    }
    public class BarangStrukturVarViewModel
    {
        public STF02 Barang { get; set; }
        public STF02E Kategori { get; set; }
        public STF20 Variant_Level_1 { get; set; }
        public STF20 Variant_Level_2 { get; set; }
        public STF20 Variant_Level_3 { get; set; }
        public List<STF02I> VariantPerMP { get; set; }
        public IList<ARF01> ListMarket { get; set; } = new List<ARF01>();
        public IList<STF20B> VariantOptMaster { get; set; } = new List<STF20B>();
    }
    public class BarangDetailVarViewModel
    {
        public IList<STF02> VariantMO { get; set; }
        public IList<STF02H> VariantMO_H { get; set; }
        public IList<ARF01> ListMarket { get; set; } = new List<ARF01>();
    }
}