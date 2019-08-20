﻿using System;
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
        //add by nurul 18/2/2019
        public List<Account> ListAccount { get; set; } = new List<Account>();
        //end add by nurul 18/2/2019
        //add by nurul 1/4/2019
        public AktivitasSubscription Payment { get; set; } = new AktivitasSubscription();
        public Account account { get; set; } = new Account();
        //end add by nurul 1/4/2019
        //public bool newPayment { get; set; }
    }
}