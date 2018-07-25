using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class RekeningViewModel
    {
        public GLFREK Rekening { get; set; }
        public List<GLFREK> ListRekening { get; set; } = new List<GLFREK>();
    }
}