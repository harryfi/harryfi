namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FAKTUR_API
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(10)]
        public string No_Faktur { get; set; }

        [StringLength(50)]
        public string No_FakturRef { get; set; }

        public DateTime? Tgl { get; set; }
        public string LUNAS { get; set; }
    }
}