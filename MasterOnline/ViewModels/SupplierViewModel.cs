using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class SupplierViewModel
    {
        public APF01 Supplier { get; set; }
        public List<APF01> ListSupplier { get; set; } = new List<APF01>();
    }
}