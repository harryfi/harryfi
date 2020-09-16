﻿using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Web;
    using MasterOnline.Models;

    public class TableTransferToFTPBarang
    {
        public string BRG { get; set; }
        public string NAMA { get; set; }
        public string BRG_SAP { get; set; }

    }
}