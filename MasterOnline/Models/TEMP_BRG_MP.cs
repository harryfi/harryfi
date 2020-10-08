using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.DynamicData;

namespace MasterOnline.Models
{
    [Table("TEMP_BRG_MP")]
    public class TEMP_BRG_MP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(200)]
        public string BRG_MP { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(255)]
        public string NAMA2 { get; set; }

        public double HJUAL { get; set; }

        public double BERAT { get; set; }

        public double PANJANG { get; set; }

        public double LEBAR { get; set; }

        public double TINGGI { get; set; }

        public string Deskripsi { get; set; }

        public int IDMARKET { get; set; }

        public double HJUAL_MP { get; set; }

        public bool DISPLAY { get; set; }

        [StringLength(10)]
        public string DeliveryTempElevenia { get; set; }

        [StringLength(50)]
        public string CATEGORY_CODE { get; set; }

        [StringLength(250)]
        public string CATEGORY_NAME { get; set; }

        [StringLength(50)]
        public string ACODE_1 { get; set; }

        [StringLength(250)]
        public string ANAME_1 { get; set; }

        //[StringLength(250)]
        public string AVALUE_1 { get; set; }

        [StringLength(50)]
        public string ACODE_2 { get; set; }

        [StringLength(250)]
        public string ANAME_2 { get; set; }

        //[StringLength(250)]
        public string AVALUE_2 { get; set; }

        [StringLength(50)]
        public string ACODE_3 { get; set; }

        [StringLength(250)]
        public string ANAME_3 { get; set; }

        //[StringLength(250)]
        public string AVALUE_3 { get; set; }

        [StringLength(50)]
        public string ACODE_4 { get; set; }

        [StringLength(250)]
        public string ANAME_4 { get; set; }

        //[StringLength(250)]
        public string AVALUE_4 { get; set; }

        [StringLength(50)]
        public string ACODE_5 { get; set; }

        [StringLength(250)]
        public string ANAME_5 { get; set; }

        //[StringLength(250)]
        public string AVALUE_5 { get; set; }

        [StringLength(50)]
        public string ACODE_6 { get; set; }

        [StringLength(250)]
        public string ANAME_6 { get; set; }

        //[StringLength(250)]
        public string AVALUE_6 { get; set; }

        [StringLength(50)]
        public string ACODE_7 { get; set; }

        [StringLength(250)]
        public string ANAME_7 { get; set; }

        //[StringLength(250)]
        public string AVALUE_7 { get; set; }

        [StringLength(50)]
        public string ACODE_8 { get; set; }

        [StringLength(250)]
        public string ANAME_8 { get; set; }

        //[StringLength(250)]
        public string AVALUE_8 { get; set; }

        [StringLength(50)]
        public string ACODE_9 { get; set; }

        [StringLength(250)]
        public string ANAME_9 { get; set; }

        //[StringLength(250)]
        public string AVALUE_9 { get; set; }

        [StringLength(50)]
        public string ACODE_10 { get; set; }

        [StringLength(250)]
        public string ANAME_10 { get; set; }

        //[StringLength(250)]
        public string AVALUE_10 { get; set; }

        [StringLength(50)]
        public string ACODE_11 { get; set; }

        [StringLength(250)]
        public string ANAME_11 { get; set; }

        //[StringLength(250)]
        public string AVALUE_11 { get; set; }

        [StringLength(50)]
        public string ACODE_12 { get; set; }

        [StringLength(250)]
        public string ANAME_12 { get; set; }

        //[StringLength(250)]
        public string AVALUE_12 { get; set; }

        [StringLength(50)]
        public string ACODE_13 { get; set; }

        [StringLength(250)]
        public string ANAME_13 { get; set; }

        //[StringLength(250)]
        public string AVALUE_13 { get; set; }

        [StringLength(50)]
        public string ACODE_14 { get; set; }

        [StringLength(250)]
        public string ANAME_14 { get; set; }

        //[StringLength(250)]
        public string AVALUE_14 { get; set; }

        [StringLength(50)]
        public string ACODE_15 { get; set; }

        [StringLength(250)]
        public string ANAME_15 { get; set; }

        //[StringLength(250)]
        public string AVALUE_15 { get; set; }

        [StringLength(50)]
        public string ACODE_16 { get; set; }

        [StringLength(250)]
        public string ANAME_16 { get; set; }

        //[StringLength(250)]
        public string AVALUE_16 { get; set; }

        [StringLength(50)]
        public string ACODE_17 { get; set; }

        [StringLength(250)]
        public string ANAME_17 { get; set; }

        //[StringLength(250)]
        public string AVALUE_17 { get; set; }

        [StringLength(50)]
        public string ACODE_18 { get; set; }

        [StringLength(250)]
        public string ANAME_18 { get; set; }

        //[StringLength(250)]
        public string AVALUE_18 { get; set; }

        [StringLength(50)]
        public string ACODE_19 { get; set; }

        [StringLength(250)]
        public string ANAME_19 { get; set; }

        //[StringLength(250)]
        public string AVALUE_19 { get; set; }

        [StringLength(50)]
        public string ACODE_20 { get; set; }

        [StringLength(250)]
        public string ANAME_20 { get; set; }

        //[StringLength(250)]
        public string AVALUE_20 { get; set; }

        [StringLength(50)]
        public string ACODE_21 { get; set; }

        [StringLength(250)]
        public string ANAME_21 { get; set; }

        //[StringLength(250)]
        public string AVALUE_21 { get; set; }

        [StringLength(50)]
        public string ACODE_22 { get; set; }

        [StringLength(250)]
        public string ANAME_22 { get; set; }

        //[StringLength(250)]
        public string AVALUE_22 { get; set; }

        [StringLength(50)]
        public string ACODE_23 { get; set; }

        [StringLength(250)]
        public string ANAME_23 { get; set; }

        //[StringLength(250)]
        public string AVALUE_23 { get; set; }

        [StringLength(50)]
        public string ACODE_24 { get; set; }

        [StringLength(250)]
        public string ANAME_24 { get; set; }

        //[StringLength(250)]
        public string AVALUE_24 { get; set; }

        [StringLength(50)]
        public string ACODE_25 { get; set; }

        [StringLength(250)]
        public string ANAME_25 { get; set; }

        //[StringLength(250)]
        public string AVALUE_25 { get; set; }

        [StringLength(50)]
        public string ACODE_26 { get; set; }

        [StringLength(250)]
        public string ANAME_26 { get; set; }

        //[StringLength(250)]
        public string AVALUE_26 { get; set; }

        [StringLength(50)]
        public string ACODE_27 { get; set; }

        [StringLength(250)]
        public string ANAME_27 { get; set; }

        //[StringLength(250)]
        public string AVALUE_27 { get; set; }

        [StringLength(50)]
        public string ACODE_28 { get; set; }

        [StringLength(250)]
        public string ANAME_28 { get; set; }

        //[StringLength(250)]
        public string AVALUE_28 { get; set; }

        [StringLength(50)]
        public string ACODE_29 { get; set; }

        [StringLength(250)]
        public string ANAME_29 { get; set; }

        //[StringLength(250)]
        public string AVALUE_29 { get; set; }

        [StringLength(50)]
        public string ACODE_30 { get; set; }

        [StringLength(250)]
        public string ANAME_30 { get; set; }

        //[StringLength(250)]
        public string AVALUE_30 { get; set; }

        [StringLength(50)]
        public string ACODE_31 { get; set; }

        [StringLength(250)]
        public string ANAME_31 { get; set; }

        //[StringLength(250)]
        public string AVALUE_31 { get; set; }

        [StringLength(50)]
        public string ACODE_32 { get; set; }

        [StringLength(250)]
        public string ANAME_32 { get; set; }

        //[StringLength(250)]
        public string AVALUE_32 { get; set; }

        [StringLength(50)]
        public string ACODE_33 { get; set; }

        [StringLength(250)]
        public string ANAME_33 { get; set; }

        //[StringLength(250)]
        public string AVALUE_33 { get; set; }

        [StringLength(50)]
        public string ACODE_34 { get; set; }

        [StringLength(250)]
        public string ANAME_34 { get; set; }

        //[StringLength(250)]
        public string AVALUE_34 { get; set; }

        [StringLength(50)]
        public string ACODE_35 { get; set; }

        [StringLength(250)]
        public string ANAME_35 { get; set; }

        //[StringLength(250)]
        public string AVALUE_35 { get; set; }

        [StringLength(50)]
        public string ACODE_36 { get; set; }

        [StringLength(250)]
        public string ANAME_36 { get; set; }

        //[StringLength(250)]
        public string AVALUE_36 { get; set; }

        [StringLength(50)]
        public string ACODE_37 { get; set; }

        [StringLength(250)]
        public string ANAME_37 { get; set; }

        //[StringLength(250)]
        public string AVALUE_37 { get; set; }

        [StringLength(50)]
        public string ACODE_38 { get; set; }

        [StringLength(250)]
        public string ANAME_38 { get; set; }

        //[StringLength(250)]
        public string AVALUE_38 { get; set; }

        [StringLength(50)]
        public string ACODE_39 { get; set; }

        [StringLength(250)]
        public string ANAME_39 { get; set; }

        //[StringLength(250)]
        [AllowHtml]
        public string AVALUE_39 { get; set; }

        [StringLength(50)]
        public string ACODE_40 { get; set; }

        [StringLength(250)]
        public string ANAME_40 { get; set; }

        //[StringLength(250)]
        public string AVALUE_40 { get; set; }

        [StringLength(50)]
        public string ACODE_41 { get; set; }

        [StringLength(250)]
        public string ANAME_41 { get; set; }

        //[StringLength(250)]
        public string AVALUE_41 { get; set; }

        [StringLength(50)]
        public string ACODE_42 { get; set; }

        [StringLength(250)]
        public string ANAME_42 { get; set; }

        //[StringLength(250)]
        public string AVALUE_42 { get; set; }

        [StringLength(50)]
        public string ACODE_43 { get; set; }

        [StringLength(250)]
        public string ANAME_43 { get; set; }

        //[StringLength(250)]
        public string AVALUE_43 { get; set; }

        [StringLength(50)]
        public string ACODE_44 { get; set; }

        [StringLength(250)]
        public string ANAME_44 { get; set; }

        //[StringLength(250)]
        public string AVALUE_44 { get; set; }

        [StringLength(50)]
        public string ACODE_45 { get; set; }

        [StringLength(250)]
        public string ANAME_45 { get; set; }

        //[StringLength(250)]
        public string AVALUE_45 { get; set; }

        [StringLength(50)]
        public string ACODE_46 { get; set; }

        [StringLength(250)]
        public string ANAME_46 { get; set; }

        //[StringLength(250)]
        public string AVALUE_46 { get; set; }

        [StringLength(50)]
        public string ACODE_47 { get; set; }

        [StringLength(250)]
        public string ANAME_47 { get; set; }

        //[StringLength(250)]
        public string AVALUE_47 { get; set; }

        [StringLength(50)]
        public string ACODE_48 { get; set; }

        [StringLength(250)]
        public string ANAME_48 { get; set; }

        //[StringLength(250)]
        public string AVALUE_48 { get; set; }

        [StringLength(50)]
        public string ACODE_49 { get; set; }

        [StringLength(250)]
        public string ANAME_49 { get; set; }

        //[StringLength(250)]
        public string AVALUE_49 { get; set; }

        [StringLength(50)]
        public string ACODE_50 { get; set; }

        [StringLength(250)]
        public string ANAME_50 { get; set; }

        //[StringLength(250)]
        public string AVALUE_50 { get; set; }

        [StringLength(30)]
        public string PICKUP_POINT { get; set; }

        [StringLength(50)]
        public string MEREK { get; set; }

        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(30)]
        public string NAMA3 { get; set; }

        public string IMAGE { get; set; }

        public string IMAGE2 { get; set; }

        public string IMAGE3 { get; set; }

        public string IMAGE4 { get; set; }

        public string IMAGE5 { get; set; }

        [StringLength(50)]
        public string SELLER_SKU { get; set; }

        [StringLength(50)]
        public string KODE_BRG_INDUK { get; set; }

        [StringLength(1)]
        public string TYPE { get; set; }

        public string BARCODE { get; set; }
        public string RAK { get; set; }
    }
    
}