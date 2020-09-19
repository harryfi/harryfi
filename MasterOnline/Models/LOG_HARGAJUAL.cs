using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterOnline.Models
{
    public class LOG_HARGAJUAL
    {
    }

    public partial class LOG_HARGAJUAL_A
    {
        [Key]
        public string NO_BUKTI { get; set; }
        public string FILE_1 { get; set; }
        public string JML_BRG_1 { get; set; }
        public int JML_BRG_NH_1 { get; set; }
        public int JML_BRG_NL_1 { get; set; }
        public string FILE_2 { get; set; }
        public string JML_BRG_2 { get; set; }
        public int JML_BRG_NH_2 { get; set; }
        public int JML_BRG_NL_2 { get; set; }
        public string FILE_3 { get; set; }
        public string JML_BRG_3 { get; set; }
        public int JML_BRG_NH_3 { get; set; }
        public int JML_BRG_NL_3 { get; set; }
        public string FILE_4 { get; set; }
        public string JML_BRG_4 { get; set; }
        public int JML_BRG_NH_4 { get; set; }
        public int JML_BRG_NL_4 { get; set; }
        public DateTime TGL_PROSES { get; set; }
        public int JAM_PROSES { get; set; }
        public int STATUS { get; set; }
        public DateTime TGL_INPUT { get; set; }
        public string USERNAME { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }

    }
    public partial class LOG_HARGAJUAL_B
    {
        [Key]
        [Column(Order = 0)]
        public string NO_BUKTI { get; set; }
        [Key]
        [Column(Order = 1)]
        public int NO_FILE { get; set; }
        public string CUST { get; set; }
        public long HANGFIRE_JOBID { get; set; }
        public string KET { get; set; }
        public DateTime TGL_INPUT { get; set; }
        public string USERNAME { get; set; }
        public string STATUS { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }

    }
}