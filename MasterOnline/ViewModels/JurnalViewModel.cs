using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class JurnalViewModel
    {
        public GLFTRAN1 Jurnal { get; set; }
        public List<GLFTRAN1> ListJurnal { get; set; } = new List<GLFTRAN1>();
        public GLFTRAN2 JurnalDetail { get; set; }
        public List<GLFTRAN2> ListJurnalDetail { get; set; } = new List<GLFTRAN2>();
        public List<GLFREK> ListRekening { get; set; } = new List<GLFREK>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}