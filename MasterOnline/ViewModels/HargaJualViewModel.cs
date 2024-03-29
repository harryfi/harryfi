﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class HargaJualViewModel
    {
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        public List<STF02H> ListHargaJualPerMarket { get; set; } = new List<STF02H>();
        public List<STF10> ListHargaTerakhir { get; set; } = new List<STF10>();
        public List<ARF01> ListPelanggan { get; set; } = new List<ARF01>();
        //add by nurul 13/6/2019
        public List<string> Errors { get; set; } = new List<string>();
        //end add by nurul 13/6/2019
    }
}