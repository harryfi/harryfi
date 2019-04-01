using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class ATTRIBUTE_JDID_AND_OPT
    {
        public List<ATTRIBUTE_JDID> attributes { get; set; } = new List<ATTRIBUTE_JDID>();
        public List<ATTRIBUTE_OPT_SHOPEE> attribute_opts { get; set; } = new List<ATTRIBUTE_OPT_SHOPEE>();
    }

    [Table("ATTRIBUTE_JDID")]
    public class ATTRIBUTE_JDID
    {
        [StringLength(50)]
        public string CATEGORY_CODE { get; set; }
        [StringLength(250)]
        public string CATEGORY_NAME { get; set; }
        [StringLength(50)]
        public string ACODE_1 { get; set; }
        [StringLength(1)]
        public string ATYPE_1 { get; set; }
        [StringLength(250)]
        public string ANAME_1 { get; set; }
        [StringLength(250)]
        public string AVALUE_1 { get; set; }
        [StringLength(50)]
        public string ACODE_2 { get; set; }
        [StringLength(1)]
        public string ATYPE_2 { get; set; }
        [StringLength(250)]
        public string ANAME_2 { get; set; }
        [StringLength(250)]
        public string AVALUE_2 { get; set; }
        [StringLength(50)]
        public string ACODE_3 { get; set; }
        [StringLength(1)]
        public string ATYPE_3 { get; set; }
        [StringLength(250)]
        public string ANAME_3 { get; set; }
        [StringLength(250)]
        public string AVALUE_3 { get; set; }
        [StringLength(50)]
        public string ACODE_4 { get; set; }
        [StringLength(1)]
        public string ATYPE_4 { get; set; }
        [StringLength(250)]
        public string ANAME_4 { get; set; }
        [StringLength(250)]
        public string AVALUE_4 { get; set; }
        [StringLength(50)]
        public string ACODE_5 { get; set; }
        [StringLength(1)]
        public string ATYPE_5 { get; set; }
        [StringLength(250)]
        public string ANAME_5 { get; set; }
        [StringLength(250)]
        public string AVALUE_5 { get; set; }
        [StringLength(50)]
        public string ACODE_6 { get; set; }
        [StringLength(1)]
        public string ATYPE_6 { get; set; }
        [StringLength(250)]
        public string ANAME_6 { get; set; }
        [StringLength(250)]
        public string AVALUE_6 { get; set; }
        [StringLength(50)]
        public string ACODE_7 { get; set; }
        [StringLength(1)]
        public string ATYPE_7 { get; set; }
        [StringLength(250)]
        public string ANAME_7 { get; set; }
        [StringLength(250)]
        public string AVALUE_7 { get; set; }
        [StringLength(50)]
        public string ACODE_8 { get; set; }
        [StringLength(1)]
        public string ATYPE_8 { get; set; }
        [StringLength(250)]
        public string ANAME_8 { get; set; }
        [StringLength(250)]
        public string AVALUE_8 { get; set; }
        [StringLength(50)]
        public string ACODE_9 { get; set; }
        [StringLength(1)]
        public string ATYPE_9 { get; set; }
        [StringLength(250)]
        public string ANAME_9 { get; set; }
        [StringLength(250)]
        public string AVALUE_9 { get; set; }
        [StringLength(50)]
        public string ACODE_10 { get; set; }
        [StringLength(1)]
        public string ATYPE_10 { get; set; }
        [StringLength(250)]
        public string ANAME_10 { get; set; }
        [StringLength(250)]
        public string AVALUE_10 { get; set; }
        [StringLength(50)]
        public string ACODE_11 { get; set; }
        [StringLength(1)]
        public string ATYPE_11 { get; set; }
        [StringLength(250)]
        public string ANAME_11 { get; set; }
        [StringLength(250)]
        public string AVALUE_11 { get; set; }
        [StringLength(50)]
        public string ACODE_12 { get; set; }
        [StringLength(1)]
        public string ATYPE_12 { get; set; }
        [StringLength(250)]
        public string ANAME_12 { get; set; }
        [StringLength(250)]
        public string AVALUE_12 { get; set; }
        [StringLength(50)]
        public string ACODE_13 { get; set; }
        [StringLength(1)]
        public string ATYPE_13 { get; set; }
        [StringLength(250)]
        public string ANAME_13 { get; set; }
        [StringLength(250)]
        public string AVALUE_13 { get; set; }
        [StringLength(50)]
        public string ACODE_14 { get; set; }
        [StringLength(1)]
        public string ATYPE_14 { get; set; }
        [StringLength(250)]
        public string ANAME_14 { get; set; }
        [StringLength(250)]
        public string AVALUE_14 { get; set; }
        [StringLength(50)]
        public string ACODE_15 { get; set; }
        [StringLength(1)]
        public string ATYPE_15 { get; set; }
        [StringLength(250)]
        public string ANAME_15 { get; set; }
        [StringLength(250)]
        public string AVALUE_15 { get; set; }
        [StringLength(50)]
        public string ACODE_16 { get; set; }
        [StringLength(1)]
        public string ATYPE_16 { get; set; }
        [StringLength(250)]
        public string ANAME_16 { get; set; }
        [StringLength(250)]
        public string AVALUE_16 { get; set; }
        [StringLength(50)]
        public string ACODE_17 { get; set; }
        [StringLength(1)]
        public string ATYPE_17 { get; set; }
        [StringLength(250)]
        public string ANAME_17 { get; set; }
        [StringLength(250)]
        public string AVALUE_17 { get; set; }
        [StringLength(50)]
        public string ACODE_18 { get; set; }
        [StringLength(1)]
        public string ATYPE_18 { get; set; }
        [StringLength(250)]
        public string ANAME_18 { get; set; }
        [StringLength(250)]
        public string AVALUE_18 { get; set; }
        [StringLength(50)]
        public string ACODE_19 { get; set; }
        [StringLength(1)]
        public string ATYPE_19 { get; set; }
        [StringLength(250)]
        public string ANAME_19 { get; set; }
        [StringLength(250)]
        public string AVALUE_19 { get; set; }
        [StringLength(50)]
        public string ACODE_20 { get; set; }
        [StringLength(1)]
        public string ATYPE_20 { get; set; }
        [StringLength(250)]
        public string ANAME_20 { get; set; }
        [StringLength(250)]
        public string AVALUE_20 { get; set; }

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
                Type myType = typeof(ATTRIBUTE_JDID);
                System.Reflection.PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(ATTRIBUTE_JDID);
                System.Reflection.PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);

            }

        }
    }

    [Table("ATTRIBUTE_OPT_JDID")]
    public class ATTRIBUTE_OPT_JDID
    {
        public ATTRIBUTE_OPT_JDID()
        {
        }
        public ATTRIBUTE_OPT_JDID(string asd, string dsa)
        {
            ACODE = asd;
            OPTION_VALUE = dsa;
        }

        [StringLength(50)]
        public string ACODE { get; set; }

        [StringLength(250)]
        public string OPTION_VALUE { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}