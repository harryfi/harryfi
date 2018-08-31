namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Category_attribute_attributeOpt_lazada : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ATTRIBUTE_LAZADA",
                c => new
                    {
                        RecNum = c.Int(nullable: false, identity: true),
                        CATEGORY_CODE = c.String(maxLength: 50),
                        CATEGORY_NAME = c.String(maxLength: 150),
                        ALABEL1 = c.String(maxLength: 50),
                        ANAME1 = c.String(maxLength: 50),
                        ATYPE1 = c.String(maxLength: 50),
                        AINPUT_TYPE1 = c.String(maxLength: 50),
                        ASALE_PROP1 = c.String(maxLength: 1),
                        AMANDATORY1 = c.String(maxLength: 1),
                        ALABEL2 = c.String(maxLength: 50),
                        ANAME2 = c.String(maxLength: 50),
                        ATYPE2 = c.String(maxLength: 50),
                        AINPUT_TYPE2 = c.String(maxLength: 50),
                        ASALE_PROP2 = c.String(maxLength: 1),
                        AMANDATORY2 = c.String(maxLength: 1),
                        ALABEL3 = c.String(maxLength: 50),
                        ANAME3 = c.String(maxLength: 50),
                        ATYPE3 = c.String(maxLength: 50),
                        AINPUT_TYPE3 = c.String(maxLength: 50),
                        ASALE_PROP3 = c.String(maxLength: 1),
                        AMANDATORY3 = c.String(maxLength: 1),
                        ALABEL4 = c.String(maxLength: 50),
                        ANAME4 = c.String(maxLength: 50),
                        ATYPE4 = c.String(maxLength: 50),
                        AINPUT_TYPE4 = c.String(maxLength: 50),
                        ASALE_PROP4 = c.String(maxLength: 1),
                        AMANDATORY4 = c.String(maxLength: 1),
                        ALABEL5 = c.String(maxLength: 50),
                        ANAME5 = c.String(maxLength: 50),
                        ATYPE5 = c.String(maxLength: 50),
                        AINPUT_TYPE5 = c.String(maxLength: 50),
                        ASALE_PROP5 = c.String(maxLength: 1),
                        AMANDATORY5 = c.String(maxLength: 1),
                        ALABEL6 = c.String(maxLength: 50),
                        ANAME6 = c.String(maxLength: 50),
                        ATYPE6 = c.String(maxLength: 50),
                        AINPUT_TYPE6 = c.String(maxLength: 50),
                        ASALE_PROP6 = c.String(maxLength: 1),
                        AMANDATORY6 = c.String(maxLength: 1),
                        ALABEL7 = c.String(maxLength: 50),
                        ANAME7 = c.String(maxLength: 50),
                        ATYPE7 = c.String(maxLength: 50),
                        AINPUT_TYPE7 = c.String(maxLength: 50),
                        ASALE_PROP7 = c.String(maxLength: 1),
                        AMANDATORY7 = c.String(maxLength: 1),
                        ALABEL8 = c.String(maxLength: 50),
                        ANAME8 = c.String(maxLength: 50),
                        ATYPE8 = c.String(maxLength: 50),
                        AINPUT_TYPE8 = c.String(maxLength: 50),
                        ASALE_PROP8 = c.String(maxLength: 1),
                        AMANDATORY8 = c.String(maxLength: 1),
                        ALABEL9 = c.String(maxLength: 50),
                        ANAME9 = c.String(maxLength: 50),
                        ATYPE9 = c.String(maxLength: 50),
                        AINPUT_TYPE9 = c.String(maxLength: 50),
                        ASALE_PROP9 = c.String(maxLength: 1),
                        AMANDATORY9 = c.String(maxLength: 1),
                        ALABEL10 = c.String(maxLength: 50),
                        ANAME10 = c.String(maxLength: 50),
                        ATYPE10 = c.String(maxLength: 50),
                        AINPUT_TYPE10 = c.String(maxLength: 50),
                        ASALE_PROP10 = c.String(maxLength: 1),
                        AMANDATORY10 = c.String(maxLength: 1),
                        ALABEL11 = c.String(maxLength: 50),
                        ANAME11 = c.String(maxLength: 50),
                        ATYPE11 = c.String(maxLength: 50),
                        AINPUT_TYPE11 = c.String(maxLength: 50),
                        ASALE_PROP11 = c.String(maxLength: 1),
                        AMANDATORY11 = c.String(maxLength: 1),
                        ALABEL12 = c.String(maxLength: 50),
                        ANAME12 = c.String(maxLength: 50),
                        ATYPE12 = c.String(maxLength: 50),
                        AINPUT_TYPE12 = c.String(maxLength: 50),
                        ASALE_PROP12 = c.String(maxLength: 1),
                        AMANDATORY12 = c.String(maxLength: 1),
                        ALABEL13 = c.String(maxLength: 50),
                        ANAME13 = c.String(maxLength: 50),
                        ATYPE13 = c.String(maxLength: 50),
                        AINPUT_TYPE13 = c.String(maxLength: 50),
                        ASALE_PROP13 = c.String(maxLength: 1),
                        AMANDATORY13 = c.String(maxLength: 1),
                        ALABEL14 = c.String(maxLength: 50),
                        ANAME14 = c.String(maxLength: 50),
                        ATYPE14 = c.String(maxLength: 50),
                        AINPUT_TYPE14 = c.String(maxLength: 50),
                        ASALE_PROP14 = c.String(maxLength: 1),
                        AMANDATORY14 = c.String(maxLength: 1),
                        ALABEL15 = c.String(maxLength: 50),
                        ANAME15 = c.String(maxLength: 50),
                        ATYPE15 = c.String(maxLength: 50),
                        AINPUT_TYPE15 = c.String(maxLength: 50),
                        ASALE_PROP15 = c.String(maxLength: 1),
                        AMANDATORY15 = c.String(maxLength: 1),
                        ALABEL16 = c.String(maxLength: 50),
                        ANAME16 = c.String(maxLength: 50),
                        ATYPE16 = c.String(maxLength: 50),
                        AINPUT_TYPE16 = c.String(maxLength: 50),
                        ASALE_PROP16 = c.String(maxLength: 1),
                        AMANDATORY16 = c.String(maxLength: 1),
                        ALABEL17 = c.String(maxLength: 50),
                        ANAME17 = c.String(maxLength: 50),
                        ATYPE17 = c.String(maxLength: 50),
                        AINPUT_TYPE17 = c.String(maxLength: 50),
                        ASALE_PROP17 = c.String(maxLength: 1),
                        AMANDATORY17 = c.String(maxLength: 1),
                        ALABEL18 = c.String(maxLength: 50),
                        ANAME18 = c.String(maxLength: 50),
                        ATYPE18 = c.String(maxLength: 50),
                        AINPUT_TYPE18 = c.String(maxLength: 50),
                        ASALE_PROP18 = c.String(maxLength: 1),
                        AMANDATORY18 = c.String(maxLength: 1),
                        ALABEL19 = c.String(maxLength: 50),
                        ANAME19 = c.String(maxLength: 50),
                        ATYPE19 = c.String(maxLength: 50),
                        AINPUT_TYPE19 = c.String(maxLength: 50),
                        ASALE_PROP19 = c.String(maxLength: 1),
                        AMANDATORY19 = c.String(maxLength: 1),
                        ALABEL20 = c.String(maxLength: 50),
                        ANAME20 = c.String(maxLength: 50),
                        ATYPE20 = c.String(maxLength: 50),
                        AINPUT_TYPE20 = c.String(maxLength: 50),
                        ASALE_PROP20 = c.String(maxLength: 1),
                        AMANDATORY20 = c.String(maxLength: 1),
                        ALABEL21 = c.String(maxLength: 50),
                        ANAME21 = c.String(maxLength: 50),
                        ATYPE21 = c.String(maxLength: 50),
                        AINPUT_TYPE21 = c.String(maxLength: 50),
                        ASALE_PROP21 = c.String(maxLength: 1),
                        AMANDATORY21 = c.String(maxLength: 1),
                        ALABEL22 = c.String(maxLength: 50),
                        ANAME22 = c.String(maxLength: 50),
                        ATYPE22 = c.String(maxLength: 50),
                        AINPUT_TYPE22 = c.String(maxLength: 50),
                        ASALE_PROP22 = c.String(maxLength: 1),
                        AMANDATORY22 = c.String(maxLength: 1),
                        ALABEL23 = c.String(maxLength: 50),
                        ANAME23 = c.String(maxLength: 50),
                        ATYPE23 = c.String(maxLength: 50),
                        AINPUT_TYPE23 = c.String(maxLength: 50),
                        ASALE_PROP23 = c.String(maxLength: 1),
                        AMANDATORY23 = c.String(maxLength: 1),
                        ALABEL24 = c.String(maxLength: 50),
                        ANAME24 = c.String(maxLength: 50),
                        ATYPE24 = c.String(maxLength: 50),
                        AINPUT_TYPE24 = c.String(maxLength: 50),
                        ASALE_PROP24 = c.String(maxLength: 1),
                        AMANDATORY24 = c.String(maxLength: 1),
                        ALABEL25 = c.String(maxLength: 50),
                        ANAME25 = c.String(maxLength: 50),
                        ATYPE25 = c.String(maxLength: 50),
                        AINPUT_TYPE25 = c.String(maxLength: 50),
                        ASALE_PROP25 = c.String(maxLength: 1),
                        AMANDATORY25 = c.String(maxLength: 1),
                        ALABEL26 = c.String(maxLength: 50),
                        ANAME26 = c.String(maxLength: 50),
                        ATYPE26 = c.String(maxLength: 50),
                        AINPUT_TYPE26 = c.String(maxLength: 50),
                        ASALE_PROP26 = c.String(maxLength: 1),
                        AMANDATORY26 = c.String(maxLength: 1),
                        ALABEL27 = c.String(maxLength: 50),
                        ANAME27 = c.String(maxLength: 50),
                        ATYPE27 = c.String(maxLength: 50),
                        AINPUT_TYPE27 = c.String(maxLength: 50),
                        ASALE_PROP27 = c.String(maxLength: 1),
                        AMANDATORY27 = c.String(maxLength: 1),
                        ALABEL28 = c.String(maxLength: 50),
                        ANAME28 = c.String(maxLength: 50),
                        ATYPE28 = c.String(maxLength: 50),
                        AINPUT_TYPE28 = c.String(maxLength: 50),
                        ASALE_PROP28 = c.String(maxLength: 1),
                        AMANDATORY28 = c.String(maxLength: 1),
                        ALABEL29 = c.String(maxLength: 50),
                        ANAME29 = c.String(maxLength: 50),
                        ATYPE29 = c.String(maxLength: 50),
                        AINPUT_TYPE29 = c.String(maxLength: 50),
                        ASALE_PROP29 = c.String(maxLength: 1),
                        AMANDATORY29 = c.String(maxLength: 1),
                        ALABEL30 = c.String(maxLength: 50),
                        ANAME30 = c.String(maxLength: 50),
                        ATYPE30 = c.String(maxLength: 50),
                        AINPUT_TYPE30 = c.String(maxLength: 50),
                        ASALE_PROP30 = c.String(maxLength: 1),
                        AMANDATORY30 = c.String(maxLength: 1),
                        ALABEL31 = c.String(maxLength: 50),
                        ANAME31 = c.String(maxLength: 50),
                        ATYPE31 = c.String(maxLength: 50),
                        AINPUT_TYPE31 = c.String(maxLength: 50),
                        ASALE_PROP31 = c.String(maxLength: 1),
                        AMANDATORY31 = c.String(maxLength: 1),
                        ALABEL32 = c.String(maxLength: 50),
                        ANAME32 = c.String(maxLength: 50),
                        ATYPE32 = c.String(maxLength: 50),
                        AINPUT_TYPE32 = c.String(maxLength: 50),
                        ASALE_PROP32 = c.String(maxLength: 1),
                        AMANDATORY32 = c.String(maxLength: 1),
                        ALABEL33 = c.String(maxLength: 50),
                        ANAME33 = c.String(maxLength: 50),
                        ATYPE33 = c.String(maxLength: 50),
                        AINPUT_TYPE33 = c.String(maxLength: 50),
                        ASALE_PROP33 = c.String(maxLength: 1),
                        AMANDATORY33 = c.String(maxLength: 1),
                        ALABEL34 = c.String(maxLength: 50),
                        ANAME34 = c.String(maxLength: 50),
                        ATYPE34 = c.String(maxLength: 50),
                        AINPUT_TYPE34 = c.String(maxLength: 50),
                        ASALE_PROP34 = c.String(maxLength: 1),
                        AMANDATORY34 = c.String(maxLength: 1),
                        ALABEL35 = c.String(maxLength: 50),
                        ANAME35 = c.String(maxLength: 50),
                        ATYPE35 = c.String(maxLength: 50),
                        AINPUT_TYPE35 = c.String(maxLength: 50),
                        ASALE_PROP35 = c.String(maxLength: 1),
                        AMANDATORY35 = c.String(maxLength: 1),
                        ALABEL36 = c.String(maxLength: 50),
                        ANAME36 = c.String(maxLength: 50),
                        ATYPE36 = c.String(maxLength: 50),
                        AINPUT_TYPE36 = c.String(maxLength: 50),
                        ASALE_PROP36 = c.String(maxLength: 1),
                        AMANDATORY36 = c.String(maxLength: 1),
                        ALABEL37 = c.String(maxLength: 50),
                        ANAME37 = c.String(maxLength: 50),
                        ATYPE37 = c.String(maxLength: 50),
                        AINPUT_TYPE37 = c.String(maxLength: 50),
                        ASALE_PROP37 = c.String(maxLength: 1),
                        AMANDATORY37 = c.String(maxLength: 1),
                        ALABEL38 = c.String(maxLength: 50),
                        ANAME38 = c.String(maxLength: 50),
                        ATYPE38 = c.String(maxLength: 50),
                        AINPUT_TYPE38 = c.String(maxLength: 50),
                        ASALE_PROP38 = c.String(maxLength: 1),
                        AMANDATORY38 = c.String(maxLength: 1),
                        ALABEL39 = c.String(maxLength: 50),
                        ANAME39 = c.String(maxLength: 50),
                        ATYPE39 = c.String(maxLength: 50),
                        AINPUT_TYPE39 = c.String(maxLength: 50),
                        ASALE_PROP39 = c.String(maxLength: 1),
                        AMANDATORY39 = c.String(maxLength: 1),
                        ALABEL40 = c.String(maxLength: 50),
                        ANAME40 = c.String(maxLength: 50),
                        ATYPE40 = c.String(maxLength: 50),
                        AINPUT_TYPE40 = c.String(maxLength: 50),
                        ASALE_PROP40 = c.String(maxLength: 1),
                        AMANDATORY40 = c.String(maxLength: 1),
                        ALABEL41 = c.String(maxLength: 50),
                        ANAME41 = c.String(maxLength: 50),
                        ATYPE41 = c.String(maxLength: 50),
                        AINPUT_TYPE41 = c.String(maxLength: 50),
                        ASALE_PROP41 = c.String(maxLength: 1),
                        AMANDATORY41 = c.String(maxLength: 1),
                        ALABEL42 = c.String(maxLength: 50),
                        ANAME42 = c.String(maxLength: 50),
                        ATYPE42 = c.String(maxLength: 50),
                        AINPUT_TYPE42 = c.String(maxLength: 50),
                        ASALE_PROP42 = c.String(maxLength: 1),
                        AMANDATORY42 = c.String(maxLength: 1),
                        ALABEL43 = c.String(maxLength: 50),
                        ANAME43 = c.String(maxLength: 50),
                        ATYPE43 = c.String(maxLength: 50),
                        AINPUT_TYPE43 = c.String(maxLength: 50),
                        ASALE_PROP43 = c.String(maxLength: 1),
                        AMANDATORY43 = c.String(maxLength: 1),
                        ALABEL44 = c.String(maxLength: 50),
                        ANAME44 = c.String(maxLength: 50),
                        ATYPE44 = c.String(maxLength: 50),
                        AINPUT_TYPE44 = c.String(maxLength: 50),
                        ASALE_PROP44 = c.String(maxLength: 1),
                        AMANDATORY44 = c.String(maxLength: 1),
                        ALABEL45 = c.String(maxLength: 50),
                        ANAME45 = c.String(maxLength: 50),
                        ATYPE45 = c.String(maxLength: 50),
                        AINPUT_TYPE45 = c.String(maxLength: 50),
                        ASALE_PROP45 = c.String(maxLength: 1),
                        AMANDATORY45 = c.String(maxLength: 1),
                        ALABEL46 = c.String(maxLength: 50),
                        ANAME46 = c.String(maxLength: 50),
                        ATYPE46 = c.String(maxLength: 50),
                        AINPUT_TYPE46 = c.String(maxLength: 50),
                        ASALE_PROP46 = c.String(maxLength: 1),
                        AMANDATORY46 = c.String(maxLength: 1),
                        ALABEL47 = c.String(maxLength: 50),
                        ANAME47 = c.String(maxLength: 50),
                        ATYPE47 = c.String(maxLength: 50),
                        AINPUT_TYPE47 = c.String(maxLength: 50),
                        ASALE_PROP47 = c.String(maxLength: 1),
                        AMANDATORY47 = c.String(maxLength: 1),
                        ALABEL48 = c.String(maxLength: 50),
                        ANAME48 = c.String(maxLength: 50),
                        ATYPE48 = c.String(maxLength: 50),
                        AINPUT_TYPE48 = c.String(maxLength: 50),
                        ASALE_PROP48 = c.String(maxLength: 1),
                        AMANDATORY48 = c.String(maxLength: 1),
                        ALABEL49 = c.String(maxLength: 50),
                        ANAME49 = c.String(maxLength: 50),
                        ATYPE49 = c.String(maxLength: 50),
                        AINPUT_TYPE49 = c.String(maxLength: 50),
                        ASALE_PROP49 = c.String(maxLength: 1),
                        AMANDATORY49 = c.String(maxLength: 1),
                        ALABEL50 = c.String(maxLength: 50),
                        ANAME50 = c.String(maxLength: 50),
                        ATYPE50 = c.String(maxLength: 50),
                        AINPUT_TYPE50 = c.String(maxLength: 50),
                        ASALE_PROP50 = c.String(maxLength: 1),
                        AMANDATORY50 = c.String(maxLength: 1),
                    })
                .PrimaryKey(t => t.RecNum);
            
            CreateTable(
                "dbo.ATTRIBUTE_OPT_LAZADA",
                c => new
                    {
                        RecNum = c.Int(nullable: false, identity: true),
                        A_NAME = c.String(maxLength: 50),
                        O_NAME = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.RecNum);
            
            CreateTable(
                "dbo.CATEGORY_LAZADA",
                c => new
                    {
                        CATEGORY_ID = c.String(nullable: false, maxLength: 50),
                        NAME = c.String(maxLength: 150),
                        LEAF = c.Boolean(nullable: false),
                        PARENT_ID = c.String(maxLength: 50),
                        RecNum = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.CATEGORY_ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CATEGORY_LAZADA");
            DropTable("dbo.ATTRIBUTE_OPT_LAZADA");
            DropTable("dbo.ATTRIBUTE_LAZADA");
        }
    }
}
