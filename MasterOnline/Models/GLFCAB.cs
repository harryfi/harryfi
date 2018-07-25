namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFCAB")]
    public partial class GLFCAB
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
