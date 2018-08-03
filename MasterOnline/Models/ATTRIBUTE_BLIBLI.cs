﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("ATTRIBUTE_BLIBLI")]
    public class ATTRIBUTE_BLIBLI
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
        [StringLength(50)]
        public string ACODE_2 { get; set; }
        [StringLength(50)]
        public string ATYPE_2 { get; set; }
        [StringLength(250)]
        public string ANAME_2 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_2 { get; set; }
        [StringLength(50)]
	    public string ACODE_3  { get; set; }
        [StringLength(50)]
	    public string ATYPE_3  { get; set; }
        [StringLength(250)]
	    public string ANAME_3  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_3  { get; set; }
        [StringLength(50)]
	    public string ACODE_4  { get; set; }
        [StringLength(50)]
	    public string ATYPE_4  { get; set; }
        [StringLength(250)]
	    public string ANAME_4  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_4  { get; set; }
        [StringLength(50)]
	    public string ACODE_5  { get; set; }
        [StringLength(50)]
        public string ATYPE_5  { get; set; }
        [StringLength(250)]
	    public string ANAME_5  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_5  { get; set; }
        [StringLength(50)]
	    public string ACODE_6  { get; set; }
        [StringLength(50)]
        public string ATYPE_6  { get; set; }
        [StringLength(250)]
	    public string ANAME_6  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_6  { get; set; }
        [StringLength(50)]
	    public string ACODE_7  { get; set; }
        [StringLength(50)]
        public string ATYPE_7  { get; set; }
        [StringLength(250)]
	    public string ANAME_7  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_7  { get; set; }
        [StringLength(50)]
	    public string ACODE_8  { get; set; }
        [StringLength(50)]
        public string ATYPE_8 { get; set; }
        [StringLength(250)]
        public string ANAME_8 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_8  { get; set; }
        [StringLength(50)]
	    public string ACODE_9  { get; set; }
        [StringLength(50)]
        public string ATYPE_9  { get; set; }
        [StringLength(250)]
	    public string ANAME_9  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_9  { get; set; }
        [StringLength(50)]
	    public string ACODE_10  { get; set; }
        [StringLength(50)]
        public string ATYPE_10  { get; set; }
        [StringLength(250)]
	    public string ANAME_10  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_10  { get; set; }
        [StringLength(50)]
	    public string ACODE_11  { get; set; }
        [StringLength(50)]
        public string ATYPE_11  { get; set; }
        [StringLength(250)]
	    public string ANAME_11  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_11  { get; set; }
        [StringLength(50)]
	    public string ACODE_12 { get; set; }
        [StringLength(50)]
        public string ATYPE_12 { get; set; }
        [StringLength(250)]
	    public string ANAME_12 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_12 { get; set; }
        [StringLength(50)]
	    public string ACODE_13  { get; set; }
        [StringLength(50)]
        public string ATYPE_13  { get; set; }
        [StringLength(250)]
	    public string ANAME_13  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_13  { get; set; }
        [StringLength(50)]
	    public string ACODE_14  { get; set; }
        [StringLength(50)]
        public string ATYPE_14  { get; set; }
        [StringLength(250)]
 	    public string ANAME_14  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_14  { get; set; }
        [StringLength(50)]
	    public string ACODE_15  { get; set; }
        [StringLength(50)]
        public string ATYPE_15  { get; set; }
        [StringLength(250)]
	    public string ANAME_15  { get; set; }
        [StringLength(1)]
        public string AOPTIONS_15  { get; set; }
        [StringLength(50)]
	    public string ACODE_16  { get; set; }
        [StringLength(50)]
        public string ATYPE_16 { get; set; }
        [StringLength(250)]
        public string ANAME_16 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_16 { get; set; }
        [StringLength(50)]
        public string ACODE_17 { get; set; }
        [StringLength(50)]
        public string ATYPE_17 { get; set; }
        [StringLength(250)]
        public string ANAME_17 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_17 { get; set; }
        [StringLength(50)]
        public string ACODE_18 { get; set; }
        [StringLength(50)]
        public string ATYPE_18 { get; set; }
        [StringLength(250)]
        public string ANAME_18 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_18 { get; set; }
        [StringLength(50)]
        public string ACODE_19 { get; set; }
        [StringLength(50)]
        public string ATYPE_19 { get; set; }
        [StringLength(250)]
        public string ANAME_19 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_19 { get; set; }
        [StringLength(50)]
        public string ACODE_20 { get; set; }
        [StringLength(50)]
        public string ATYPE_20 { get; set; }
        [StringLength(250)]
        public string ANAME_20 { get; set; }
        [StringLength(1)]
        public string AOPTIONS_20 { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}