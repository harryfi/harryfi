using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("MIDTRANS_DATA")]
    public class MIDTRANS_DATA
    {
        [StringLength(50)]
        public string BANK { get; set; }

        [StringLength(50)]
        public string TRANSACTION_TIME { get; set; }

        [StringLength(50)]
        public string GROSS_AMOUNT { get; set; }

        [StringLength(50)]
        public string ORDER_ID { get; set; }

        [StringLength(50)]
        public string PAYMENT_TYPE { get; set; }

        [StringLength(250)]
        public string SIGNATURE_KEY { get; set; }

        [StringLength(50)]
        public string STATUS_CODE { get; set; }

        [StringLength(50)]
        public string TRANSACTION_ID { get; set; }

        [StringLength(50)]
        public string TRANSACTION_STATUS { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}