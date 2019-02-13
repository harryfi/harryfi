using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class MenuAccountViewModel
    {
        public List<Account> ListAccount { get; set; } = new List<Account>();
        public List<Partner> ListPartner { get; set; } = new List<Partner>();
    }
}