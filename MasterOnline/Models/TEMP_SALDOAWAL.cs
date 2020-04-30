using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.DynamicData;

namespace MasterOnline.Models
{
    [Table("TEMP_SALDOAWAL")]
    public class TEMP_SALDOAWAL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(40)]
        public string BRG { get; set; }

        public double QTY { get; set; }

        public double HARGA_SATUAN { get; set; }

    }

}