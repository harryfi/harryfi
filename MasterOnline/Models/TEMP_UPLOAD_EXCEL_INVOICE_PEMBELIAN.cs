namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_UPLOAD_EXCEL_INVOICE_PEMBELIAN
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }

        [StringLength(200)]
        public string NOBUK { get; set; }

        [StringLength(200)]
        public string NOREF { get; set; }

        public DateTime TGL { get; set; }

        [StringLength(100)]
        public string KODE_SUPPLIER { get; set; }

        public int TOP { get; set; }

        public double PPN { get; set; }

        public double ONGKIR { get; set; }

        [StringLength(100)]
        public string KODE_BRG { get; set; }

        public string GUDANG { get; set; }

        public int QTY { get; set; }

        public double HARGA_SATUAN { get; set; }

        public double TOTAL_NILAI_DISC { get; set; }

        public double TOTAL { get; set; }

    }
}
