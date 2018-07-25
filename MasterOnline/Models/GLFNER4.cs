namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFNER4
    {
        [Key]
        [StringLength(2)]
        public string KODE_KOLOM { get; set; }

        [StringLength(30)]
        public string KET { get; set; }

        [StringLength(1)]
        public string PERSEN { get; set; }

        [StringLength(15)]
        public string J1 { get; set; }

        [StringLength(15)]
        public string J2 { get; set; }

        [StringLength(15)]
        public string J3 { get; set; }

        [StringLength(15)]
        public string J4 { get; set; }

        [StringLength(15)]
        public string J5 { get; set; }

        [StringLength(15)]
        public string J6 { get; set; }

        [StringLength(15)]
        public string J7 { get; set; }

        [StringLength(15)]
        public string J8 { get; set; }

        [StringLength(15)]
        public string J9 { get; set; }

        [StringLength(15)]
        public string J10 { get; set; }

        [StringLength(15)]
        public string J11 { get; set; }

        [StringLength(15)]
        public string J12 { get; set; }

        [StringLength(15)]
        public string J13 { get; set; }

        [StringLength(2)]
        public string T1 { get; set; }

        [StringLength(2)]
        public string T2 { get; set; }

        [StringLength(2)]
        public string T3 { get; set; }

        [StringLength(2)]
        public string T4 { get; set; }

        [StringLength(2)]
        public string T5 { get; set; }

        [StringLength(2)]
        public string T6 { get; set; }

        [StringLength(2)]
        public string T7 { get; set; }

        [StringLength(2)]
        public string T8 { get; set; }

        [StringLength(2)]
        public string T9 { get; set; }

        [StringLength(2)]
        public string T10 { get; set; }

        [StringLength(2)]
        public string T11 { get; set; }

        [StringLength(2)]
        public string T12 { get; set; }

        [StringLength(2)]
        public string T13 { get; set; }

        public short TH1 { get; set; }

        public short TH2 { get; set; }

        public short TH3 { get; set; }

        public short TH4 { get; set; }

        public short TH5 { get; set; }

        public short TH6 { get; set; }

        public short TH7 { get; set; }

        public short TH8 { get; set; }

        public short TH9 { get; set; }

        public short TH10 { get; set; }

        public short TH11 { get; set; }

        public short TH12 { get; set; }

        public short TH13 { get; set; }

        public short DR1 { get; set; }

        public short DR2 { get; set; }

        public short DR3 { get; set; }

        public short DR4 { get; set; }

        public short DR5 { get; set; }

        public short DR6 { get; set; }

        public short DR7 { get; set; }

        public short DR8 { get; set; }

        public short DR9 { get; set; }

        public short DR10 { get; set; }

        public short DR11 { get; set; }

        public short DR12 { get; set; }

        public short DR13 { get; set; }

        public short SD1 { get; set; }

        public short SD2 { get; set; }

        public short SD3 { get; set; }

        public short SD4 { get; set; }

        public short SD5 { get; set; }

        public short SD6 { get; set; }

        public short SD7 { get; set; }

        public short SD8 { get; set; }

        public short SD9 { get; set; }

        public short SD10 { get; set; }

        public short SD11 { get; set; }

        public short SD12 { get; set; }

        public short SD13 { get; set; }
    }
}
