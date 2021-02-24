using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("BUKALAPAK_TOKEN")]
    public class BUKALAPAK_TOKEN
    {

        [Key]
        [Column(Order = 0)]
        public string ACCOUNT { get; set; }

        [Key]
        [Column(Order = 1)]
        public string CUST { get; set; }
        //public string TOKEN { get; set; }
        //public long EXPIRED { get; set; }
        //public string REFRESH_TOKEN { get; set; }
        public DateTime CREATED_AT { get; set; }
        //public string TOKEN_TYPE { get; set; }
        //public DateTime REQUEST_DATE { get; set; }
        public string CODE { get; set; }
        public string EMAIL { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }
    }
}