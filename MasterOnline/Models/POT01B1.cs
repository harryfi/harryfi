namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT01B1
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string PO_NO { get; set; }

        public byte? UNIT_CODE { get; set; }

        [Required]
        [StringLength(1)]
        public string JENIS_SIZE { get; set; }

        public double QUANTITY1 { get; set; }

        public double QUANTITY2 { get; set; }

        public double QUANTITY3 { get; set; }

        public double QUANTITY4 { get; set; }

        public double QUANTITY5 { get; set; }

        public double QUANTITY6 { get; set; }

        public double QUANTITY7 { get; set; }

        public double QUANTITY8 { get; set; }

        public double QUANTITY9 { get; set; }

        public double QUANTITY10 { get; set; }

        public double QUANTITY11 { get; set; }

        public double QUANTITY12 { get; set; }

        public double QUANTITY13 { get; set; }

        public double QUANTITY14 { get; set; }

        public double QUANTITY15 { get; set; }

        public double APRICE { get; set; }

        public double PRICE { get; set; }

        public double DISC_PERCENT { get; set; }

        public double DISC_VALUE { get; set; }

        public double TOTAL { get; set; }

        public double ACCEPT_QUANTITY1 { get; set; }

        public double ACCEPT_QUANTITY2 { get; set; }

        public double ACCEPT_QUANTITY3 { get; set; }

        public double ACCEPT_QUANTITY4 { get; set; }

        public double ACCEPT_QUANTITY5 { get; set; }

        public double ACCEPT_QUANTITY6 { get; set; }

        public double ACCEPT_QUANTITY7 { get; set; }

        public double ACCEPT_QUANTITY8 { get; set; }

        public double ACCEPT_QUANTITY9 { get; set; }

        public double ACCEPT_QUANTITY10 { get; set; }

        public double ACCEPT_QUANTITY11 { get; set; }

        public double ACCEPT_QUANTITY12 { get; set; }

        public double ACCEPT_QUANTITY13 { get; set; }

        public double ACCEPT_QUANTITY14 { get; set; }

        public double ACCEPT_QUANTITY15 { get; set; }

        public DateTime ACCEPT_DATE { get; set; }

        public double BACK_QUANTITY1 { get; set; }

        public double BACK_QUANTITY2 { get; set; }

        public double BACK_QUANTITY3 { get; set; }

        public double BACK_QUANTITY4 { get; set; }

        public double BACK_QUANTITY5 { get; set; }

        public double BACK_QUANTITY6 { get; set; }

        public double BACK_QUANTITY7 { get; set; }

        public double BACK_QUANTITY8 { get; set; }

        public double BACK_QUANTITY9 { get; set; }

        public double BACK_QUANTITY10 { get; set; }

        public double BACK_QUANTITY11 { get; set; }

        public double BACK_QUANTITY12 { get; set; }

        public double BACK_QUANTITY13 { get; set; }

        public double BACK_QUANTITY14 { get; set; }

        public double BACK_QUANTITY15 { get; set; }

        [StringLength(10)]
        public string POP_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NO_URUT { get; set; }

        public double? BIAYA { get; set; }

        public bool WRITE_KONFIG { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        [StringLength(250)]
        public string KET { get; set; }

        public double? DISC_PERCENT_1 { get; set; }

        public double? DISC_PERCENT_2 { get; set; }

        public double? DISC_PERCENT_3 { get; set; }

        public double? DISC_VALUE_1 { get; set; }

        public double? DISC_VALUE_2 { get; set; }

        public double? DISC_VALUE_3 { get; set; }

        public double PPNBM_PERCENT { get; set; }

        public double PPNBM_VALUE { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT1 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT2 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT3 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT4 { get; set; }
    }
}
