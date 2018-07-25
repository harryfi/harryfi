namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF04
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string Faktur { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime Tgl_Proses { get; set; }

        public double Saldo { get; set; }

        public double Tukar_Lama { get; set; }

        public double Tukar_Baru { get; set; }
    }
}
