using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class MenuAccount
    {
        //add by nurul 20/2/2019
        public Account Account { get; set; }
        //end add by nurul 20/2/2019
        public List<Account> ListAccount { get; set; } = new List<Account>();
        public List<Partner> ListPartner { get; set; } = new List<Partner>();
        
    }
}