namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFNER2
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(2)]
        public string KODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string KKN { get; set; }

        [StringLength(30)]
        public string NKN { get; set; }

        [StringLength(1)]
        public string AKTIVA { get; set; }

        [StringLength(1)]
        public string CETAK_KEL { get; set; }

        [StringLength(30)]
        public string NAMA_KEL { get; set; }

        public bool KONSOLIDASI { get; set; }

        [StringLength(1)]
        public string Hitung_Kel { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }
    }
}
