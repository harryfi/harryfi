namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STFSYS_M
    {
        [StringLength(1)]
        public string Level_NonSize1 { get; set; }

        [StringLength(1)]
        public string Level_NonSize2 { get; set; }

        [StringLength(1)]
        public string Level_NonSize3 { get; set; }

        [StringLength(1)]
        public string Level_NonSize4 { get; set; }

        [StringLength(1)]
        public string Level_Size { get; set; }

        [StringLength(10)]
        public string Sort_Size1 { get; set; }

        [StringLength(10)]
        public string Sort_Size2 { get; set; }

        [StringLength(10)]
        public string Sort_Size3 { get; set; }

        [StringLength(10)]
        public string Sort_Size4 { get; set; }

        [StringLength(10)]
        public string Sort_Size5 { get; set; }

        [StringLength(10)]
        public string Sort_Size6 { get; set; }

        [StringLength(10)]
        public string Sort_Size7 { get; set; }

        [StringLength(10)]
        public string Sort_Size8 { get; set; }

        [StringLength(10)]
        public string Sort_Size9 { get; set; }

        [StringLength(10)]
        public string Sort_Size10 { get; set; }

        [StringLength(10)]
        public string Sort_Size11 { get; set; }

        [StringLength(10)]
        public string Sort_Size12 { get; set; }

        [StringLength(10)]
        public string Sort_Size13 { get; set; }

        [StringLength(10)]
        public string Sort_Size14 { get; set; }

        [StringLength(10)]
        public string Sort_Size15 { get; set; }

        [StringLength(10)]
        public string Sort_Size16 { get; set; }

        [StringLength(10)]
        public string Sort_Size17 { get; set; }

        [StringLength(10)]
        public string Sort_Size18 { get; set; }

        [StringLength(10)]
        public string Sort_Size19 { get; set; }

        [StringLength(10)]
        public string Sort_Size20 { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string Level_NonSize5 { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(1)]
        public string Level_NonSize6 { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(1)]
        public string Level_NonSize7 { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(1)]
        public string Level_NonSize8 { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(1)]
        public string Level_NonSize9 { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(2)]
        public string Level_NonSize10 { get; set; }
    }
}
