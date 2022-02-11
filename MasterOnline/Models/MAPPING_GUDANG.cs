
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

namespace MasterOnline.Models
{
    [Table("MAPPING_GUDANG")]
    public partial class MAPPING_GUDANG
    {
        [StringLength(20)]
        public string CUST { get; set; }

        [StringLength(100)]
        public string GD_MO { get; set; }

        [StringLength(100)]
        public String GD_MP { get; set; }

        [StringLength(200)]
        public string NAMA_GD_MP { get; set; }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }
    }

}
