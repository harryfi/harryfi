using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class TransferToFTPParameterLinkFtpViewModel
    {
        public LINKFTP LINKFTP { get; set; }
        //public List<LINKFTP> ListLINKFTP { get; set; } = new List<LINKFTP>();

        public string statusAddonFTP { get; set; }
    }
}