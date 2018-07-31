namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add3Tables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ATTRIBUTE_BLIBLI",
                c => new
                    {
                        RecNum = c.Int(nullable: false, identity: true),
                        CATEGORY_CODE = c.String(maxLength: 50),
                        CATEGORY_NAME = c.String(maxLength: 250),
                        ACODE_1 = c.String(maxLength: 50),
                        ATYPE_1 = c.String(maxLength: 50),
                        ANAME_1 = c.String(maxLength: 250),
                        AOPTIONS_1 = c.String(maxLength: 1),
                        ACODE_2 = c.String(maxLength: 50),
                        ATYPE_2 = c.String(maxLength: 50),
                        ANAME_2 = c.String(maxLength: 250),
                        AOPTIONS_2 = c.String(maxLength: 1),
                        ACODE_3 = c.String(maxLength: 50),
                        ATYPE_3 = c.String(maxLength: 50),
                        ANAME_3 = c.String(maxLength: 250),
                        AOPTIONS_3 = c.String(maxLength: 1),
                        ACODE_4 = c.String(maxLength: 50),
                        ATYPE_4 = c.String(maxLength: 50),
                        ANAME_4 = c.String(maxLength: 250),
                        AOPTIONS_4 = c.String(maxLength: 1),
                        ACODE_5 = c.String(maxLength: 50),
                        ATYPE_5 = c.String(maxLength: 50),
                        ANAME_5 = c.String(maxLength: 250),
                        AOPTIONS_5 = c.String(maxLength: 1),
                        ACODE_6 = c.String(maxLength: 50),
                        ATYPE_6 = c.String(maxLength: 50),
                        ANAME_6 = c.String(maxLength: 250),
                        AOPTIONS_6 = c.String(maxLength: 1),
                        ACODE_7 = c.String(maxLength: 50),
                        ATYPE_7 = c.String(maxLength: 50),
                        ANAME_7 = c.String(maxLength: 250),
                        AOPTIONS_7 = c.String(maxLength: 1),
                        ACODE_8 = c.String(maxLength: 50),
                        ATYPE_8 = c.String(maxLength: 50),
                        ANAME_8 = c.String(maxLength: 250),
                        AOPTIONS_8 = c.String(maxLength: 1),
                        ACODE_9 = c.String(maxLength: 50),
                        ATYPE_9 = c.String(maxLength: 50),
                        ANAME_9 = c.String(maxLength: 250),
                        AOPTIONS_9 = c.String(maxLength: 1),
                        ACODE_10 = c.String(maxLength: 50),
                        ATYPE_10 = c.String(maxLength: 50),
                        ANAME_10 = c.String(maxLength: 250),
                        AOPTIONS_10 = c.String(maxLength: 1),
                        ACODE_11 = c.String(maxLength: 50),
                        ATYPE_11 = c.String(maxLength: 50),
                        ANAME_11 = c.String(maxLength: 250),
                        AOPTIONS_11 = c.String(maxLength: 1),
                        ACODE_12 = c.String(maxLength: 50),
                        ATYPE_12 = c.String(maxLength: 50),
                        ANAME_12 = c.String(maxLength: 250),
                        AOPTIONS_12 = c.String(maxLength: 1),
                        ACODE_13 = c.String(maxLength: 50),
                        ATYPE_13 = c.String(maxLength: 50),
                        ANAME_13 = c.String(maxLength: 250),
                        AOPTIONS_13 = c.String(maxLength: 1),
                        ACODE_14 = c.String(maxLength: 50),
                        ATYPE_14 = c.String(maxLength: 50),
                        ANAME_14 = c.String(maxLength: 250),
                        AOPTIONS_14 = c.String(maxLength: 1),
                        ACODE_15 = c.String(maxLength: 50),
                        ATYPE_15 = c.String(maxLength: 50),
                        ANAME_15 = c.String(maxLength: 250),
                        AOPTIONS_15 = c.String(maxLength: 1),
                        ACODE_16 = c.String(maxLength: 50),
                        ATYPE_16 = c.String(maxLength: 50),
                        ANAME_16 = c.String(maxLength: 250),
                        AOPTIONS_16 = c.String(maxLength: 1),
                        ACODE_17 = c.String(maxLength: 50),
                        ATYPE_17 = c.String(maxLength: 50),
                        ANAME_17 = c.String(maxLength: 250),
                        AOPTIONS_17 = c.String(maxLength: 1),
                        ACODE_18 = c.String(maxLength: 50),
                        ATYPE_18 = c.String(maxLength: 50),
                        ANAME_18 = c.String(maxLength: 250),
                        AOPTIONS_18 = c.String(maxLength: 1),
                        ACODE_19 = c.String(maxLength: 50),
                        ATYPE_19 = c.String(maxLength: 50),
                        ANAME_19 = c.String(maxLength: 250),
                        AOPTIONS_19 = c.String(maxLength: 1),
                        ACODE_20 = c.String(maxLength: 50),
                        ATYPE_20 = c.String(maxLength: 50),
                        ANAME_20 = c.String(maxLength: 250),
                        AOPTIONS_20 = c.String(maxLength: 1),
                    })
                .PrimaryKey(t => t.RecNum);
            
            CreateTable(
                "dbo.ATTRIBUTE_OPT_BLIBLI",
                c => new
                    {
                        RecNum = c.Int(nullable: false, identity: true),
                        ACODE = c.String(maxLength: 50),
                        ATYPE = c.String(maxLength: 50),
                        ANAME = c.String(maxLength: 250),
                        OPTION_VALUE = c.String(maxLength: 250),
                    })
                .PrimaryKey(t => t.RecNum);
            
            CreateTable(
                "dbo.CATEGORY_BLIBLI",
                c => new
                    {
                        CATEGORY_CODE = c.String(nullable: false, maxLength: 50),
                        CATEGORY_NAME = c.String(maxLength: 250),
                        PARENT_CODE = c.String(maxLength: 50),
                        IS_LAST_NODE = c.String(maxLength: 1),
                        MASTER_CATEGORY_CODE = c.String(maxLength: 50),
                        RecNum = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.CATEGORY_CODE);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CATEGORY_BLIBLI");
            DropTable("dbo.ATTRIBUTE_OPT_BLIBLI");
            DropTable("dbo.ATTRIBUTE_BLIBLI");
        }
    }
}
