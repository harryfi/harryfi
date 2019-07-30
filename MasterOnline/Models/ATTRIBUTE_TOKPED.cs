using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("ATTRIBUTE_TOKPED")]
    public class ATTRIBUTE_TOKPED
    {
        public object this[string propertyName]
        {
            get
            {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(ATTRIBUTE_TOKPED);
                System.Reflection.PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(ATTRIBUTE_TOKPED);
                System.Reflection.PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);

            }

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(50)]
        public string CATEGORY_CODE { get; set; }
        [StringLength(250)]
        public string CATEGORY_NAME { get; set; }
        
        public int? VARIANT_ID_1 { get; set; }
        public int? HAS_UNIT_1 { get; set; }
        [StringLength(250)]
        public string ANAME_1 { get; set; }
        [StringLength(1)]
        public string STATUS_1 { get; set; }
        
        public int? VARIANT_ID_2 { get; set; }
        public int? HAS_UNIT_2 { get; set; }
        [StringLength(250)]
        public string ANAME_2 { get; set; }
        [StringLength(1)]
        public string STATUS_2 { get; set; }
        
	    public int? VARIANT_ID_3  { get; set; }
	    public int? HAS_UNIT_3  { get; set; }
        [StringLength(250)]
	    public string ANAME_3  { get; set; }
        [StringLength(1)]
        public string STATUS_3  { get; set; }
        
	    public int? VARIANT_ID_4  { get; set; }
	    public int? HAS_UNIT_4  { get; set; }
        [StringLength(250)]
	    public string ANAME_4  { get; set; }
        [StringLength(1)]
        public string STATUS_4  { get; set; }
        
	    public int? VARIANT_ID_5  { get; set; }
        public int? HAS_UNIT_5  { get; set; }
        [StringLength(250)]
	    public string ANAME_5  { get; set; }
        [StringLength(1)]
        public string STATUS_5  { get; set; }
        
	    public int? VARIANT_ID_6  { get; set; }
        public int? HAS_UNIT_6  { get; set; }
        [StringLength(250)]
	    public string ANAME_6  { get; set; }
        [StringLength(1)]
        public string STATUS_6  { get; set; }
        
	    public int? VARIANT_ID_7  { get; set; }
        public int? HAS_UNIT_7  { get; set; }
        [StringLength(250)]
	    public string ANAME_7  { get; set; }
        [StringLength(1)]
        public string STATUS_7  { get; set; }
        
	    public int? VARIANT_ID_8  { get; set; }
        public int? HAS_UNIT_8 { get; set; }
        [StringLength(250)]
        public string ANAME_8 { get; set; }
        [StringLength(1)]
        public string STATUS_8  { get; set; }
        
	    public int? VARIANT_ID_9  { get; set; }
        public int? HAS_UNIT_9  { get; set; }
        [StringLength(250)]
	    public string ANAME_9  { get; set; }
        [StringLength(1)]
        public string STATUS_9  { get; set; }
        
	    public int? VARIANT_ID_10  { get; set; }
        public int? HAS_UNIT_10  { get; set; }
        [StringLength(250)]
	    public string ANAME_10  { get; set; }
        [StringLength(1)]
        public string STATUS_10  { get; set; }
        
	    public int? VARIANT_ID_11  { get; set; }
        public int? HAS_UNIT_11  { get; set; }
        [StringLength(250)]
	    public string ANAME_11  { get; set; }
        [StringLength(1)]
        public string STATUS_11  { get; set; }
        
	    public int? VARIANT_ID_12 { get; set; }
        public int? HAS_UNIT_12 { get; set; }
        [StringLength(250)]
	    public string ANAME_12 { get; set; }
        [StringLength(1)]
        public string STATUS_12 { get; set; }
        
	    public int? VARIANT_ID_13  { get; set; }
        public int? HAS_UNIT_13  { get; set; }
        [StringLength(250)]
	    public string ANAME_13  { get; set; }
        [StringLength(1)]
        public string STATUS_13  { get; set; }
        
	    public int? VARIANT_ID_14  { get; set; }
        public int? HAS_UNIT_14  { get; set; }
        [StringLength(250)]
 	    public string ANAME_14  { get; set; }
        [StringLength(1)]
        public string STATUS_14  { get; set; }
        
	    public int? VARIANT_ID_15  { get; set; }
        public int? HAS_UNIT_15  { get; set; }
        [StringLength(250)]
	    public string ANAME_15  { get; set; }
        [StringLength(1)]
        public string STATUS_15  { get; set; }
        
	    public int? VARIANT_ID_16  { get; set; }
        public int? HAS_UNIT_16 { get; set; }
        [StringLength(250)]
        public string ANAME_16 { get; set; }
        [StringLength(1)]
        public string STATUS_16 { get; set; }
        
        public int? VARIANT_ID_17 { get; set; }
        public int? HAS_UNIT_17 { get; set; }
        [StringLength(250)]
        public string ANAME_17 { get; set; }
        [StringLength(1)]
        public string STATUS_17 { get; set; }
        
        public int? VARIANT_ID_18 { get; set; }
        public int? HAS_UNIT_18 { get; set; }
        [StringLength(250)]
        public string ANAME_18 { get; set; }
        [StringLength(1)]
        public string STATUS_18 { get; set; }
        
        public int? VARIANT_ID_19 { get; set; }
        public int? HAS_UNIT_19 { get; set; }
        [StringLength(250)]
        public string ANAME_19 { get; set; }
        [StringLength(1)]
        public string STATUS_19 { get; set; }
        
        public int? VARIANT_ID_20 { get; set; }
        public int? HAS_UNIT_20 { get; set; }
        [StringLength(250)]
        public string ANAME_20 { get; set; }
        [StringLength(1)]
        public string STATUS_20 { get; set; }
        
        public int? VARIANT_ID_21 { get; set; }
        public int? HAS_UNIT_21 { get; set; }
        [StringLength(250)]
        public string ANAME_21 { get; set; }
        [StringLength(1)]
        public string STATUS_21 { get; set; }
        
        public int? VARIANT_ID_22 { get; set; }
        public int? HAS_UNIT_22 { get; set; }
        [StringLength(250)]
        public string ANAME_22 { get; set; }
        [StringLength(1)]
        public string STATUS_22 { get; set; }
        
        public int? VARIANT_ID_23 { get; set; }
        public int? HAS_UNIT_23 { get; set; }
        [StringLength(250)]
        public string ANAME_23 { get; set; }
        [StringLength(1)]
        public string STATUS_23 { get; set; }
        
        public int? VARIANT_ID_24 { get; set; }
        public int? HAS_UNIT_24 { get; set; }
        [StringLength(250)]
        public string ANAME_24 { get; set; }
        [StringLength(1)]
        public string STATUS_24 { get; set; }
        
        public int? VARIANT_ID_25 { get; set; }
        public int? HAS_UNIT_25 { get; set; }
        [StringLength(250)]
        public string ANAME_25 { get; set; }
        [StringLength(1)]
        public string STATUS_25 { get; set; }
        
        public int? VARIANT_ID_26 { get; set; }
        public int? HAS_UNIT_26 { get; set; }
        [StringLength(250)]
        public string ANAME_26 { get; set; }
        [StringLength(1)]
        public string STATUS_26 { get; set; }
        
        public int? VARIANT_ID_27 { get; set; }
        public int? HAS_UNIT_27 { get; set; }
        [StringLength(250)]
        public string ANAME_27 { get; set; }
        [StringLength(1)]
        public string STATUS_27 { get; set; }
        
        public int? VARIANT_ID_28 { get; set; }
        public int? HAS_UNIT_28 { get; set; }
        [StringLength(250)]
        public string ANAME_28 { get; set; }
        [StringLength(1)]
        public string STATUS_28 { get; set; }
        
        public int? VARIANT_ID_29 { get; set; }
        public int? HAS_UNIT_29 { get; set; }
        [StringLength(250)]
        public string ANAME_29 { get; set; }
        [StringLength(1)]
        public string STATUS_29 { get; set; }
        
        public int? VARIANT_ID_30 { get; set; }
        public int? HAS_UNIT_30 { get; set; }
        [StringLength(250)]
        public string ANAME_30 { get; set; }
        [StringLength(1)]
        public string STATUS_30 { get; set; }
    }
}