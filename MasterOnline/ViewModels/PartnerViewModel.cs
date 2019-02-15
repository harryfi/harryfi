using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class PartnerViewModel
    {
        public List<Partner> ListPartner { get; set; } = new List<Partner>();

        //add by nurul 15/2/2019
        public Partner partner { get; set; } = new Partner();
        //end add by nurul 15/2/2019
    }
}