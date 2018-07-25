using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class AccountUserViewModel
    {
        public Account Account { get; set; }
        public User User { get; set; }
        public Admin Admin { get; set; }
        public List<User> ListUser { get; set; } = new List<User>();
        public List<SecUser> ListSec { get; set; } = new List<SecUser>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}