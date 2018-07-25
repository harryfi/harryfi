using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class MerkBarangViewModel
    {
        public STF02E Merk { get; set; }
        public List<STF02E> ListMerk { get; set; } = new List<STF02E>();
    }
}