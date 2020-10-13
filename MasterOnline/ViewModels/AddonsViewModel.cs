using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class AddonsViewModel
    {
        public Addons Addons { get; set; }
        public IList<Addons> ListAddons { get; set; } = new List<Addons>();
    }
}