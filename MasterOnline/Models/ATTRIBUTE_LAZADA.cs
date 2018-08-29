using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("ATTRIBUTE_LAZADA")]
    public class ATTRIBUTE_LAZADA
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(50)]
        public string CATEGORY_CODE { get; set; }
        [StringLength(150)]
        public string CATEGORY_NAME { get; set; }

        [StringLength(50)]
        public string ALABEL1 { get; set; }
        [StringLength(50)]
        public string ANAME1 { get; set; }
        [StringLength(50)]
        public string ATYPE1 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE1 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP1 { get; set; }
        [StringLength(1)]
        public string AMANDATORY1 { get; set; }

        [StringLength(50)]
        public string ALABEL2 { get; set; }
        [StringLength(50)]
        public string ANAME2 { get; set; }
        [StringLength(50)]
        public string ATYPE2 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE2 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP2 { get; set; }
        [StringLength(1)]
        public string AMANDATORY2 { get; set; }

        [StringLength(50)]
        public string ALABEL3 { get; set; }
        [StringLength(50)]
        public string ANAME3 { get; set; }
        [StringLength(50)]
        public string ATYPE3 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE3 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP3 { get; set; }
        [StringLength(1)]
        public string AMANDATORY3 { get; set; }

        [StringLength(50)]
        public string ALABEL4 { get; set; }
        [StringLength(50)]
        public string ANAME4 { get; set; }
        [StringLength(50)]
        public string ATYPE4 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE4 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP4 { get; set; }
        [StringLength(1)]
        public string AMANDATORY4 { get; set; }

        [StringLength(50)]
        public string ALABEL5 { get; set; }
        [StringLength(50)]
        public string ANAME5 { get; set; }
        [StringLength(50)]
        public string ATYPE5 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE5 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP5 { get; set; }
        [StringLength(1)]
        public string AMANDATORY5 { get; set; }

        [StringLength(50)]
        public string ALABEL6 { get; set; }
        [StringLength(50)]
        public string ANAME6 { get; set; }
        [StringLength(50)]
        public string ATYPE6 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE6 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP6 { get; set; }
        [StringLength(1)]
        public string AMANDATORY6 { get; set; }

        [StringLength(50)]
        public string ALABEL7 { get; set; }
        [StringLength(50)]
        public string ANAME7 { get; set; }
        [StringLength(50)]
        public string ATYPE7 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE7 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP7 { get; set; }
        [StringLength(1)]
        public string AMANDATORY7 { get; set; }

        [StringLength(50)]
        public string ALABEL8 { get; set; }
        [StringLength(50)]
        public string ANAME8 { get; set; }
        [StringLength(50)]
        public string ATYPE8 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE8 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP8 { get; set; }
        [StringLength(1)]
        public string AMANDATORY8 { get; set; }

        [StringLength(50)]
        public string ALABEL9 { get; set; }
        [StringLength(50)]
        public string ANAME9 { get; set; }
        [StringLength(50)]
        public string ATYPE9 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE9 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP9 { get; set; }
        [StringLength(1)]
        public string AMANDATORY9 { get; set; }

        [StringLength(50)]
        public string ALABEL10 { get; set; }
        [StringLength(50)]
        public string ANAME10 { get; set; }
        [StringLength(50)]
        public string ATYPE10 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE10 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP10 { get; set; }
        [StringLength(1)]
        public string AMANDATORY10 { get; set; }

        [StringLength(50)]
        public string ALABEL11 { get; set; }
        [StringLength(50)]
        public string ANAME11 { get; set; }
        [StringLength(50)]
        public string ATYPE11 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE11 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP11 { get; set; }
        [StringLength(1)]
        public string AMANDATORY11 { get; set; }

        [StringLength(50)]
        public string ALABEL12 { get; set; }
        [StringLength(50)]
        public string ANAME12 { get; set; }
        [StringLength(50)]
        public string ATYPE12 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE12 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP12 { get; set; }
        [StringLength(1)]
        public string AMANDATORY12 { get; set; }

        [StringLength(50)]
        public string ALABEL13 { get; set; }
        [StringLength(50)]
        public string ANAME13 { get; set; }
        [StringLength(50)]
        public string ATYPE13 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE13 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP13 { get; set; }
        [StringLength(1)]
        public string AMANDATORY13 { get; set; }

        [StringLength(50)]
        public string ALABEL14 { get; set; }
        [StringLength(50)]
        public string ANAME14 { get; set; }
        [StringLength(50)]
        public string ATYPE14 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE14 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP14 { get; set; }
        [StringLength(1)]
        public string AMANDATORY14 { get; set; }

        [StringLength(50)]
        public string ALABEL15 { get; set; }
        [StringLength(50)]
        public string ANAME15 { get; set; }
        [StringLength(50)]
        public string ATYPE15 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE15 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP15 { get; set; }
        [StringLength(1)]
        public string AMANDATORY15 { get; set; }

        [StringLength(50)]
        public string ALABEL16 { get; set; }
        [StringLength(50)]
        public string ANAME16 { get; set; }
        [StringLength(50)]
        public string ATYPE16 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE16 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP16 { get; set; }
        [StringLength(1)]
        public string AMANDATORY16 { get; set; }

        [StringLength(50)]
        public string ALABEL17 { get; set; }
        [StringLength(50)]
        public string ANAME17 { get; set; }
        [StringLength(50)]
        public string ATYPE17 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE17 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP17 { get; set; }
        [StringLength(1)]
        public string AMANDATORY17 { get; set; }

        [StringLength(50)]
        public string ALABEL18 { get; set; }
        [StringLength(50)]
        public string ANAME18 { get; set; }
        [StringLength(50)]
        public string ATYPE18 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE18 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP18 { get; set; }
        [StringLength(1)]
        public string AMANDATORY18 { get; set; }

        [StringLength(50)]
        public string ALABEL19 { get; set; }
        [StringLength(50)]
        public string ANAME19 { get; set; }
        [StringLength(50)]
        public string ATYPE19 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE19 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP19 { get; set; }
        [StringLength(1)]
        public string AMANDATORY19 { get; set; }

        [StringLength(50)]
        public string ALABEL20 { get; set; }
        [StringLength(50)]
        public string ANAME20 { get; set; }
        [StringLength(50)]
        public string ATYPE20 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE20 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP20 { get; set; }
        [StringLength(1)]
        public string AMANDATORY20 { get; set; }

        [StringLength(50)]
        public string ALABEL21 { get; set; }
        [StringLength(50)]
        public string ANAME21 { get; set; }
        [StringLength(50)]
        public string ATYPE21 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE21 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP21 { get; set; }
        [StringLength(1)]
        public string AMANDATORY21 { get; set; }

        [StringLength(50)]
        public string ALABEL22 { get; set; }
        [StringLength(50)]
        public string ANAME22 { get; set; }
        [StringLength(50)]
        public string ATYPE22 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE22 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP22 { get; set; }
        [StringLength(1)]
        public string AMANDATORY22 { get; set; }

        [StringLength(50)]
        public string ALABEL23 { get; set; }
        [StringLength(50)]
        public string ANAME23 { get; set; }
        [StringLength(50)]
        public string ATYPE23 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE23 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP23 { get; set; }
        [StringLength(1)]
        public string AMANDATORY23 { get; set; }

        [StringLength(50)]
        public string ALABEL24 { get; set; }
        [StringLength(50)]
        public string ANAME24 { get; set; }
        [StringLength(50)]
        public string ATYPE24 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE24 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP24 { get; set; }
        [StringLength(1)]
        public string AMANDATORY24 { get; set; }

        [StringLength(50)]
        public string ALABEL25 { get; set; }
        [StringLength(50)]
        public string ANAME25 { get; set; }
        [StringLength(50)]
        public string ATYPE25 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE25 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP25 { get; set; }
        [StringLength(1)]
        public string AMANDATORY25 { get; set; }

        [StringLength(50)]
        public string ALABEL26 { get; set; }
        [StringLength(50)]
        public string ANAME26 { get; set; }
        [StringLength(50)]
        public string ATYPE26 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE26 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP26 { get; set; }
        [StringLength(1)]
        public string AMANDATORY26 { get; set; }

        [StringLength(50)]
        public string ALABEL27 { get; set; }
        [StringLength(50)]
        public string ANAME27 { get; set; }
        [StringLength(50)]
        public string ATYPE27 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE27 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP27 { get; set; }
        [StringLength(1)]
        public string AMANDATORY27 { get; set; }

        [StringLength(50)]
        public string ALABEL28 { get; set; }
        [StringLength(50)]
        public string ANAME28 { get; set; }
        [StringLength(50)]
        public string ATYPE28 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE28 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP28 { get; set; }
        [StringLength(1)]
        public string AMANDATORY28 { get; set; }

        [StringLength(50)]
        public string ALABEL29 { get; set; }
        [StringLength(50)]
        public string ANAME29 { get; set; }
        [StringLength(50)]
        public string ATYPE29 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE29 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP29 { get; set; }
        [StringLength(1)]
        public string AMANDATORY29 { get; set; }

        [StringLength(50)]
        public string ALABEL30 { get; set; }
        [StringLength(50)]
        public string ANAME30 { get; set; }
        [StringLength(50)]
        public string ATYPE30 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE30 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP30 { get; set; }
        [StringLength(1)]
        public string AMANDATORY30 { get; set; }

        [StringLength(50)]
        public string ALABEL31 { get; set; }
        [StringLength(50)]
        public string ANAME31 { get; set; }
        [StringLength(50)]
        public string ATYPE31 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE31 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP31 { get; set; }
        [StringLength(1)]
        public string AMANDATORY31 { get; set; }

        [StringLength(50)]
        public string ALABEL32 { get; set; }
        [StringLength(50)]
        public string ANAME32 { get; set; }
        [StringLength(50)]
        public string ATYPE32 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE32 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP32 { get; set; }
        [StringLength(1)]
        public string AMANDATORY32 { get; set; }

        [StringLength(50)]
        public string ALABEL33 { get; set; }
        [StringLength(50)]
        public string ANAME33 { get; set; }
        [StringLength(50)]
        public string ATYPE33 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE33 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP33 { get; set; }
        [StringLength(1)]
        public string AMANDATORY33 { get; set; }

        [StringLength(50)]
        public string ALABEL34 { get; set; }
        [StringLength(50)]
        public string ANAME34 { get; set; }
        [StringLength(50)]
        public string ATYPE34 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE34 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP34 { get; set; }
        [StringLength(1)]
        public string AMANDATORY34 { get; set; }

        [StringLength(50)]
        public string ALABEL35 { get; set; }
        [StringLength(50)]
        public string ANAME35 { get; set; }
        [StringLength(50)]
        public string ATYPE35 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE35 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP35 { get; set; }
        [StringLength(1)]
        public string AMANDATORY35 { get; set; }

        [StringLength(50)]
        public string ALABEL36 { get; set; }
        [StringLength(50)]
        public string ANAME36 { get; set; }
        [StringLength(50)]
        public string ATYPE36 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE36 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP36 { get; set; }
        [StringLength(1)]
        public string AMANDATORY36 { get; set; }

        [StringLength(50)]
        public string ALABEL37 { get; set; }
        [StringLength(50)]
        public string ANAME37 { get; set; }
        [StringLength(50)]
        public string ATYPE37 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE37 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP37 { get; set; }
        [StringLength(1)]
        public string AMANDATORY37 { get; set; }

        [StringLength(50)]
        public string ALABEL38 { get; set; }
        [StringLength(50)]
        public string ANAME38 { get; set; }
        [StringLength(50)]
        public string ATYPE38 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE38 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP38 { get; set; }
        [StringLength(1)]
        public string AMANDATORY38 { get; set; }

        [StringLength(50)]
        public string ALABEL39 { get; set; }
        [StringLength(50)]
        public string ANAME39 { get; set; }
        [StringLength(50)]
        public string ATYPE39 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE39 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP39 { get; set; }
        [StringLength(1)]
        public string AMANDATORY39 { get; set; }

        [StringLength(50)]
        public string ALABEL40 { get; set; }
        [StringLength(50)]
        public string ANAME40 { get; set; }
        [StringLength(50)]
        public string ATYPE40 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE40 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP40 { get; set; }
        [StringLength(1)]
        public string AMANDATORY40 { get; set; }

        [StringLength(50)]
        public string ALABEL41 { get; set; }
        [StringLength(50)]
        public string ANAME41 { get; set; }
        [StringLength(50)]
        public string ATYPE41 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE41 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP41 { get; set; }
        [StringLength(1)]
        public string AMANDATORY41 { get; set; }

        [StringLength(50)]
        public string ALABEL42 { get; set; }
        [StringLength(50)]
        public string ANAME42 { get; set; }
        [StringLength(50)]
        public string ATYPE42 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE42 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP42 { get; set; }
        [StringLength(1)]
        public string AMANDATORY42 { get; set; }

        [StringLength(50)]
        public string ALABEL43 { get; set; }
        [StringLength(50)]
        public string ANAME43 { get; set; }
        [StringLength(50)]
        public string ATYPE43 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE43 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP43 { get; set; }
        [StringLength(1)]
        public string AMANDATORY43 { get; set; }

        [StringLength(50)]
        public string ALABEL44 { get; set; }
        [StringLength(50)]
        public string ANAME44 { get; set; }
        [StringLength(50)]
        public string ATYPE44 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE44 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP44 { get; set; }
        [StringLength(1)]
        public string AMANDATORY44 { get; set; }

        [StringLength(50)]
        public string ALABEL45 { get; set; }
        [StringLength(50)]
        public string ANAME45 { get; set; }
        [StringLength(50)]
        public string ATYPE45 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE45 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP45 { get; set; }
        [StringLength(1)]
        public string AMANDATORY45 { get; set; }

        [StringLength(50)]
        public string ALABEL46 { get; set; }
        [StringLength(50)]
        public string ANAME46 { get; set; }
        [StringLength(50)]
        public string ATYPE46 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE46 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP46 { get; set; }
        [StringLength(1)]
        public string AMANDATORY46 { get; set; }

        [StringLength(50)]
        public string ALABEL47 { get; set; }
        [StringLength(50)]
        public string ANAME47 { get; set; }
        [StringLength(50)]
        public string ATYPE47 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE47 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP47 { get; set; }
        [StringLength(1)]
        public string AMANDATORY47 { get; set; }

        [StringLength(50)]
        public string ALABEL48 { get; set; }
        [StringLength(50)]
        public string ANAME48 { get; set; }
        [StringLength(50)]
        public string ATYPE48 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE48 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP48 { get; set; }
        [StringLength(1)]
        public string AMANDATORY48 { get; set; }

        [StringLength(50)]
        public string ALABEL49 { get; set; }
        [StringLength(50)]
        public string ANAME49 { get; set; }
        [StringLength(50)]
        public string ATYPE49 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE49 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP49 { get; set; }
        [StringLength(1)]
        public string AMANDATORY49 { get; set; }

        [StringLength(50)]
        public string ALABEL50 { get; set; }
        [StringLength(50)]
        public string ANAME50 { get; set; }
        [StringLength(50)]
        public string ATYPE50 { get; set; }
        [StringLength(50)]
        public string AINPUT_TYPE50 { get; set; }
        [StringLength(1)]
        public string ASALE_PROP50 { get; set; }
        [StringLength(1)]
        public string AMANDATORY50 { get; set; }
    }
}