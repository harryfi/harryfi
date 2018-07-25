using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class SecurityUserViewModel
    {
        public User User { get; set; }
        public List<User> ListUser { get; set; }
        public List<FormMos> ListForms { get; set; } = new List<FormMos>();
        public SecUser SecUser { get; set; }
        public List<SecUser> ListSec { get; set; } = new List<SecUser>();
    }
}