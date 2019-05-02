using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class ATTRIBUTE_BL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }


        [StringLength(50)]
        public string CATEGORY_CODE { get; set; }

        public string FIELDNAME_1 { get; set; }
        public string DISPLAYNAME_1 { get; set; }
        public string INPUTTYPE_1 { get; set; }
        public bool REQUIRED_1 { get; set; }

        public string FIELDNAME_2 { get; set; }
        public string DISPLAYNAME_2 { get; set; }
        public string INPUTTYPE_2 { get; set; }
        public bool REQUIRED_2 { get; set; }

        public string FIELDNAME_3 { get; set; }
        public string DISPLAYNAME_3 { get; set; }
        public string INPUTTYPE_3 { get; set; }
        public bool REQUIRED_3 { get; set; }

        public string FIELDNAME_4 { get; set; }
        public string DISPLAYNAME_4 { get; set; }
        public string INPUTTYPE_4 { get; set; }
        public bool REQUIRED_4 { get; set; }

        public string FIELDNAME_5 { get; set; }
        public string DISPLAYNAME_5 { get; set; }
        public string INPUTTYPE_5 { get; set; }
        public bool REQUIRED_5 { get; set; }

        public string FIELDNAME_6 { get; set; }
        public string DISPLAYNAME_6 { get; set; }
        public string INPUTTYPE_6 { get; set; }
        public bool REQUIRED_6 { get; set; }

        public string FIELDNAME_7 { get; set; }
        public string DISPLAYNAME_7 { get; set; }
        public string INPUTTYPE_7 { get; set; }
        public bool REQUIRED_7 { get; set; }

        public string FIELDNAME_8 { get; set; }
        public string DISPLAYNAME_8 { get; set; }
        public string INPUTTYPE_8 { get; set; }
        public bool REQUIRED_8 { get; set; }

        public string FIELDNAME_9 { get; set; }
        public string DISPLAYNAME_9 { get; set; }
        public string INPUTTYPE_9 { get; set; }
        public bool REQUIRED_9 { get; set; }

        public string FIELDNAME_10 { get; set; }
        public string DISPLAYNAME_10 { get; set; }
        public string INPUTTYPE_10 { get; set; }
        public bool REQUIRED_10 { get; set; }


        public string FIELDNAME_11 { get; set; }
        public string DISPLAYNAME_11 { get; set; }
        public string INPUTTYPE_11 { get; set; }
        public bool REQUIRED_11 { get; set; }

        public string FIELDNAME_12 { get; set; }
        public string DISPLAYNAME_12 { get; set; }
        public string INPUTTYPE_12 { get; set; }
        public bool REQUIRED_12 { get; set; }

        public string FIELDNAME_13 { get; set; }
        public string DISPLAYNAME_13 { get; set; }
        public string INPUTTYPE_13 { get; set; }
        public bool REQUIRED_13 { get; set; }

        public string FIELDNAME_14 { get; set; }
        public string DISPLAYNAME_14 { get; set; }
        public string INPUTTYPE_14 { get; set; }
        public bool REQUIRED_14 { get; set; }

        public string FIELDNAME_15 { get; set; }
        public string DISPLAYNAME_15 { get; set; }
        public string INPUTTYPE_15 { get; set; }
        public bool REQUIRED_15 { get; set; }

        public string FIELDNAME_16 { get; set; }
        public string DISPLAYNAME_16 { get; set; }
        public string INPUTTYPE_16 { get; set; }
        public bool REQUIRED_16 { get; set; }

        public string FIELDNAME_17 { get; set; }
        public string DISPLAYNAME_17 { get; set; }
        public string INPUTTYPE_17 { get; set; }
        public bool REQUIRED_17 { get; set; }

        public string FIELDNAME_18 { get; set; }
        public string DISPLAYNAME_18 { get; set; }
        public string INPUTTYPE_18 { get; set; }
        public bool REQUIRED_18 { get; set; }

        public string FIELDNAME_19 { get; set; }
        public string DISPLAYNAME_19 { get; set; }
        public string INPUTTYPE_19 { get; set; }
        public bool REQUIRED_19 { get; set; }

        public string FIELDNAME_20 { get; set; }
        public string DISPLAYNAME_20 { get; set; }
        public string INPUTTYPE_20 { get; set; }
        public bool REQUIRED_20 { get; set; }

        public string FIELDNAME_21 { get; set; }
        public string DISPLAYNAME_21 { get; set; }
        public string INPUTTYPE_21 { get; set; }
        public bool REQUIRED_21 { get; set; }

        public string FIELDNAME_22 { get; set; }
        public string DISPLAYNAME_22 { get; set; }
        public string INPUTTYPE_22 { get; set; }
        public bool REQUIRED_22 { get; set; }

        public string FIELDNAME_23 { get; set; }
        public string DISPLAYNAME_23 { get; set; }
        public string INPUTTYPE_23 { get; set; }
        public bool REQUIRED_23 { get; set; }

        public string FIELDNAME_24 { get; set; }
        public string DISPLAYNAME_24 { get; set; }
        public string INPUTTYPE_24 { get; set; }
        public bool REQUIRED_24 { get; set; }

        public string FIELDNAME_25 { get; set; }
        public string DISPLAYNAME_25 { get; set; }
        public string INPUTTYPE_25 { get; set; }
        public bool REQUIRED_25 { get; set; }

        public string FIELDNAME_26 { get; set; }
        public string DISPLAYNAME_26 { get; set; }
        public string INPUTTYPE_26 { get; set; }
        public bool REQUIRED_26 { get; set; }

        public string FIELDNAME_27 { get; set; }
        public string DISPLAYNAME_27 { get; set; }
        public string INPUTTYPE_27 { get; set; }
        public bool REQUIRED_27 { get; set; }

        public string FIELDNAME_28 { get; set; }
        public string DISPLAYNAME_28 { get; set; }
        public string INPUTTYPE_28 { get; set; }
        public bool REQUIRED_28 { get; set; }

        public string FIELDNAME_29 { get; set; }
        public string DISPLAYNAME_29 { get; set; }
        public string INPUTTYPE_29 { get; set; }
        public bool REQUIRED_29 { get; set; }

        public string FIELDNAME_30 { get; set; }
        public string DISPLAYNAME_30 { get; set; }
        public string INPUTTYPE_30 { get; set; }
        public bool REQUIRED_30 { get; set; }

        public object this[string propertyName]
        {
            get
            {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(ATTRIBUTE_BL);
                System.Reflection.PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(ATTRIBUTE_BL);
                System.Reflection.PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);

            }

        }
    }

    public class ATTRIBUTE_OPT_BL
    {

        [StringLength(50)]
        public string CATEGORY_CODE { get; set; }

        [StringLength(50)]
        public string ID { get; set; }

        [StringLength(100)]
        public string VALUE { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}