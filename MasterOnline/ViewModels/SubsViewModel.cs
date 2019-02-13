using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class SubsViewModel
    {
        public Subscription Subs { get; set; } = new Subscription();
        public List<Subscription> ListSubs { get; set; } = new List<Subscription>();
        public List<AktivitasSubscription> ListAktivitasSubs { get; set; } = new List<AktivitasSubscription>();
        public bool loggedin { get; set; }
    }
}