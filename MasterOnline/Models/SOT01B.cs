namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SOT01B
    {
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [Key]
        public int? NO_URUT { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(20)]
        public string BRG_CUST { get; set; }

        [StringLength(10)]
        public string SATUAN { get; set; }

        public double H_SATUAN { get; set; }

        public double QTY { get; set; }

        public double DISCOUNT { get; set; }

        public double NILAI_DISC { get; set; }

        public double HARGA { get; set; }

        public bool WRITE_KONFIG { get; set; }

        [StringLength(50)]
        public string ORDER_ITEM_ID { get; set; }

        [StringLength(50)]
        public string STATUS_BRG { get; set; }

        public double QTY_KIRIM { get; set; }

        public double QTY_RETUR { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL_INPUT { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL_KIRIM { get; set; }

        [StringLength(10)]
        public string LOKASI { get; set; }

        public double DISCOUNT_2 { get; set; }

        public double DISCOUNT_3 { get; set; }

        public double DISCOUNT_4 { get; set; }

        public double DISCOUNT_5 { get; set; }

        public double NILAI_DISC_1 { get; set; }

        public double NILAI_DISC_2 { get; set; }

        public double NILAI_DISC_3 { get; set; }

        public double NILAI_DISC_4 { get; set; }

        public double NILAI_DISC_5 { get; set; }

        [StringLength(255)]
        public string CATATAN { get; set; }

        public double? TRANS_NO_URUT { get; set; }

        public int? SATUAN_N { get; set; }

        public double? QTY_N { get; set; }

        public double? NTITIPAN { get; set; }

        public double? DISC_TITIPAN { get; set; }

        public double TOTAL { get; set; }

        public double PPN { get; set; }

        public double NETTO { get; set; }

        //add by nurul 5/5/2020, temp ket tokped 
        [Column(TypeName = "text")]
        public string KET_DETAIL { get; set; }
        //end add by nurul 5/5/2020, temp ket tokped

        //ADD BY NURUL 9/9/2020
        [StringLength(20)]
        public string BRG_MULTISKU { get; set; }
        //END ADD BY NURUL 9/9/2020
    }
}
