namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TRF03
    {
        [Key]
        [StringLength(10)]
        public string NO_POLISI { get; set; }

        [StringLength(1)]
        public string UKURAN { get; set; }

        [StringLength(50)]
        public string USERNAME { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        [StringLength(10)]
        public string KD_CC { get; set; }

        [StringLength(50)]
        public string NM_PEMILIK { get; set; }

        [StringLength(30)]
        public string MEREK { get; set; }

        [StringLength(30)]
        public string TYPE { get; set; }

        [StringLength(30)]
        public string JENIS { get; set; }

        [StringLength(30)]
        public string MODEL { get; set; }

        [StringLength(30)]
        public string WARNA { get; set; }

        [StringLength(4)]
        public string THN_BUAT { get; set; }

        [StringLength(30)]
        public string NO_MESIN { get; set; }

        [StringLength(30)]
        public string NO_RANGKA { get; set; }

        [StringLength(20)]
        public string WARNA_TNKB { get; set; }

        [StringLength(30)]
        public string NO_BPKB { get; set; }

        [StringLength(10)]
        public string SILINDER { get; set; }

        public DateTime? MS_BERLAKU { get; set; }

        [StringLength(20)]
        public string UK_CHASIS { get; set; }

        [StringLength(10)]
        public string NO_CODE { get; set; }

        public DateTime? TGL_KIR { get; set; }
    }
}
