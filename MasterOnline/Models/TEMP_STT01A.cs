namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_STT01A
    {
        [Key]
        [Column(Order = 0)]
        public byte Jenis_Form { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string Nobuk { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime Tgl { get; set; }

        [StringLength(1)]
        public string Satuan { get; set; }

        [StringLength(256)]
        public string Ket { get; set; }

        [StringLength(1)]
        public string ST_Cetak { get; set; }

        [StringLength(1)]
        public string ST_Posting { get; set; }

        [StringLength(2)]
        public string JTran { get; set; }

        [StringLength(1)]
        public string MK { get; set; }

        [StringLength(1)]
        public string JRef { get; set; }

        [StringLength(10)]
        public string Ref { get; set; }

        [StringLength(30)]
        public string UserName { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime TglInput { get; set; }

        [Key]
        [Column(Order = 4)]
        public bool Retur_Penuh { get; set; }

        [Key]
        [Column(Order = 5)]
        public bool Terima_Penuh { get; set; }

        [StringLength(3)]
        public string VALUTA { get; set; }

        [Key]
        [Column(Order = 6)]
        public double TUKAR { get; set; }

        public DateTime? TGL_TERIMA_CUST { get; set; }

        public DateTime? TGL_RETUR { get; set; }

        [Key]
        [Column(Order = 7)]
        public int ID { get; set; }

        [StringLength(15)]
        public string WORK_CENTER { get; set; }

        [StringLength(1)]
        public string STATUS_LOADING { get; set; }

        [Key]
        [Column(Order = 8)]
        public double NO_URUT_SJ { get; set; }
    }
}
