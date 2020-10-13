using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class AddonsCustomerViewModel
    {
        public Addons_Customer Addons_Customer { get; set; } = new Addons_Customer();
        public IList<Addons_Customer> ListCustAddons { get; set; } = new List<Addons_Customer>();
        //public List<Addons> ListAddons { get; set; } = new List<Addons>();
        //public List<Account> Accounts { get; set; } = new List<Account>();

        public Account Accounts { get; set; } = new Account();
        public Addons Addons { get; set; } = new Addons();
    }
}