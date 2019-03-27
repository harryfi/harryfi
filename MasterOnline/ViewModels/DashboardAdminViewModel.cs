using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class DashboardAdminViewModel
    {
        public List<Account> ListAccount { get; set; } = new List<Account>();
        public List<AktivitasSubscription> ListSales { get; set; } = new List<AktivitasSubscription>();

        public int? Three { get; set; }
        public int? Twelve { get; set; }
        public double Bayar { get; set; }
        
    }
}