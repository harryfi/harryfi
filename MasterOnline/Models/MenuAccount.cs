using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class MenuAccount
    {
        public List<Account> ListAccount { get; set; } = new List<Account>();
        public List<Partner> ListPartner { get; set; } = new List<Partner>();
    }
}