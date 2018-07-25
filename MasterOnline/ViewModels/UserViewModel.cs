using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class UserViewModel
    {
        public List<Account> ListAccount { get; set; } = new List<Account>();
        public List<User> ListUser { get; set; } = new List<User>();
    }
}