using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class ATTRIBUTE_SHOPEE_AND_OPT {
        public List<ATTRIBUTE_SHOPEE> attributes { get; set; } = new List<ATTRIBUTE_SHOPEE>();
        public List<ATTRIBUTE_OPT_SHOPEE> attribute_opts { get; set; } = new List<ATTRIBUTE_OPT_SHOPEE>();
    }
    [Table("ATTRIBUTE_SHOPEE")]
    public class ATTRIBUTE_SHOPEE
    {
        [StringLength(50)]
        public string CATEGORY_CODE { get; set; }
        [StringLength(250)]
        public string CATEGORY_NAME { get; set; }
        [StringLength(50)]
        public string ACODE_1 { get; set; }
        [StringLength(50)]
        public string ATYPE_1 { get; set; }
        [StringLength(250)]
        public string ANAME_1 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_1 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_1 { get; set; }
        [StringLength(50)]
        public string ACODE_2 { get; set; }
        [StringLength(50)]
        public string ATYPE_2 { get; set; }
        [StringLength(250)]
        public string ANAME_2 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_2 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_2 { get; set; }
        [StringLength(50)]
	    public string ACODE_3  { get; set; }
        [StringLength(50)]
	    public string ATYPE_3  { get; set; }
        [StringLength(250)]
	    public string ANAME_3  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_3  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_3 { get; set; }
        [StringLength(50)]
	    public string ACODE_4  { get; set; }
        [StringLength(50)]
	    public string ATYPE_4  { get; set; }
        [StringLength(250)]
	    public string ANAME_4  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_4  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_4 { get; set; }
        [StringLength(50)]
	    public string ACODE_5  { get; set; }
        [StringLength(50)]
        public string ATYPE_5  { get; set; }
        [StringLength(250)]
	    public string ANAME_5  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_5  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_5 { get; set; }
        [StringLength(50)]
	    public string ACODE_6  { get; set; }
        [StringLength(50)]
        public string ATYPE_6  { get; set; }
        [StringLength(250)]
	    public string ANAME_6  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_6  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_6 { get; set; }
        [StringLength(50)]
	    public string ACODE_7  { get; set; }
        [StringLength(50)]
        public string ATYPE_7  { get; set; }
        [StringLength(250)]
	    public string ANAME_7  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_7  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_7 { get; set; }
        [StringLength(50)]
	    public string ACODE_8  { get; set; }
        [StringLength(50)]
        public string ATYPE_8 { get; set; }
        [StringLength(250)]
        public string ANAME_8 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_8  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_8 { get; set; }
        [StringLength(50)]
	    public string ACODE_9  { get; set; }
        [StringLength(50)]
        public string ATYPE_9  { get; set; }
        [StringLength(250)]
	    public string ANAME_9  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_9  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_9 { get; set; }
        [StringLength(50)]
	    public string ACODE_10  { get; set; }
        [StringLength(50)]
        public string ATYPE_10  { get; set; }
        [StringLength(250)]
	    public string ANAME_10  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_10  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_10 { get; set; }
        [StringLength(50)]
	    public string ACODE_11  { get; set; }
        [StringLength(50)]
        public string ATYPE_11  { get; set; }
        [StringLength(250)]
	    public string ANAME_11  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_11  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_11 { get; set; }
        [StringLength(50)]
	    public string ACODE_12 { get; set; }
        [StringLength(50)]
        public string ATYPE_12 { get; set; }
        [StringLength(250)]
	    public string ANAME_12 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_12 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_12 { get; set; }
        [StringLength(50)]
	    public string ACODE_13  { get; set; }
        [StringLength(50)]
        public string ATYPE_13  { get; set; }
        [StringLength(250)]
	    public string ANAME_13  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_13  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_13 { get; set; }
        [StringLength(50)]
	    public string ACODE_14  { get; set; }
        [StringLength(50)]
        public string ATYPE_14  { get; set; }
        [StringLength(250)]
 	    public string ANAME_14  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_14  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_14 { get; set; }
        [StringLength(50)]
	    public string ACODE_15  { get; set; }
        [StringLength(50)]
        public string ATYPE_15  { get; set; }
        [StringLength(250)]
	    public string ANAME_15  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_15  { get; set; }
        [StringLength(1)]
        public string AMANDATORY_15 { get; set; }
        [StringLength(50)]
	    public string ACODE_16  { get; set; }
        [StringLength(50)]
        public string ATYPE_16 { get; set; }
        [StringLength(250)]
        public string ANAME_16 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_16 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_16 { get; set; }
        [StringLength(50)]
        public string ACODE_17 { get; set; }
        [StringLength(50)]
        public string ATYPE_17 { get; set; }
        [StringLength(250)]
        public string ANAME_17 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_17 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_17 { get; set; }
        [StringLength(50)]
        public string ACODE_18 { get; set; }
        [StringLength(50)]
        public string ATYPE_18 { get; set; }
        [StringLength(250)]
        public string ANAME_18 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_18 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_18 { get; set; }
        [StringLength(50)]
        public string ACODE_19 { get; set; }
        [StringLength(50)]
        public string ATYPE_19 { get; set; }
        [StringLength(250)]
        public string ANAME_19 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_19 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_19 { get; set; }
        [StringLength(50)]
        public string ACODE_20 { get; set; }
        [StringLength(50)]
        public string ATYPE_20 { get; set; }
        [StringLength(250)]
        public string ANAME_20 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_20 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_20 { get; set; }
        [StringLength(50)]
        public string ACODE_21 { get; set; }
        [StringLength(50)]
        public string ATYPE_21 { get; set; }
        [StringLength(250)]
        public string ANAME_21 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_21 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_21 { get; set; }
        [StringLength(50)]
        public string ACODE_22 { get; set; }
        [StringLength(50)]
        public string ATYPE_22 { get; set; }
        [StringLength(250)]
        public string ANAME_22 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_22 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_22 { get; set; }
        [StringLength(50)]
        public string ACODE_23 { get; set; }
        [StringLength(50)]
        public string ATYPE_23 { get; set; }
        [StringLength(250)]
        public string ANAME_23 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_23 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_23 { get; set; }
        [StringLength(50)]
        public string ACODE_24 { get; set; }
        [StringLength(50)]
        public string ATYPE_24 { get; set; }
        [StringLength(250)]
        public string ANAME_24 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_24 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_24 { get; set; }
        [StringLength(50)]
        public string ACODE_25 { get; set; }
        [StringLength(50)]
        public string ATYPE_25 { get; set; }
        [StringLength(250)]
        public string ANAME_25 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_25 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_25 { get; set; }
        [StringLength(50)]
        public string ACODE_26 { get; set; }
        [StringLength(50)]
        public string ATYPE_26 { get; set; }
        [StringLength(250)]
        public string ANAME_26 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_26 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_26 { get; set; }
        [StringLength(50)]
        public string ACODE_27 { get; set; }
        [StringLength(50)]
        public string ATYPE_27 { get; set; }
        [StringLength(250)]
        public string ANAME_27 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_27 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_27 { get; set; }
        [StringLength(50)]
        public string ACODE_28 { get; set; }
        [StringLength(50)]
        public string ATYPE_28 { get; set; }
        [StringLength(250)]
        public string ANAME_28 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_28 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_28 { get; set; }
        [StringLength(50)]
        public string ACODE_29 { get; set; }
        [StringLength(50)]
        public string ATYPE_29 { get; set; }
        [StringLength(250)]
        public string ANAME_29 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_29 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_29 { get; set; }
        [StringLength(50)]
        public string ACODE_30 { get; set; }
        [StringLength(50)]
        public string ATYPE_30 { get; set; }
        [StringLength(250)]
        public string ANAME_30 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_30 { get; set; }
        [StringLength(1)]
        public string AMANDATORY_30 { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }


        public object this[string propertyName]
        {
            get
            {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(ATTRIBUTE_SHOPEE);
                System.Reflection.PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(ATTRIBUTE_SHOPEE);
                System.Reflection.PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);

            }

        }
    }
}