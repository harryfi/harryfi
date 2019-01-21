using System;
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
}