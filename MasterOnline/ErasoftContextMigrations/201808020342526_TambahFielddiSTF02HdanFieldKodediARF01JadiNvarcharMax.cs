namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TambahFielddiSTF02HdanFieldKodediARF01JadiNvarcharMax : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.STF02H", "CATEGORY_CODE", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "CATEGORY_NAME", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_1", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_1", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_1", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_2", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_2", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_2", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_3", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_3", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_3", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_4", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_4", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_4", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_5", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_5", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_5", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_6", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_6", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_6", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_7", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_7", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_7", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_8", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_8", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_8", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_9", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_9", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_9", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_10", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_10", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_10", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_11", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_11", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_11", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_12", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_12", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_12", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_13", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_13", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_13", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_14", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_14", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_14", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_15", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_15", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_15", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_16", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_16", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_16", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_17", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_17", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_17", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_18", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_18", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_18", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_19", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_19", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_19", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_20", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_20", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_20", c => c.String(maxLength: 250));
            AlterColumn("dbo.ARF01", "Kode", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ARF01", "Kode", c => c.String(maxLength: 50, unicode: false));
            DropColumn("dbo.STF02H", "AVALUE_20");
            DropColumn("dbo.STF02H", "ANAME_20");
            DropColumn("dbo.STF02H", "ACODE_20");
            DropColumn("dbo.STF02H", "AVALUE_19");
            DropColumn("dbo.STF02H", "ANAME_19");
            DropColumn("dbo.STF02H", "ACODE_19");
            DropColumn("dbo.STF02H", "AVALUE_18");
            DropColumn("dbo.STF02H", "ANAME_18");
            DropColumn("dbo.STF02H", "ACODE_18");
            DropColumn("dbo.STF02H", "AVALUE_17");
            DropColumn("dbo.STF02H", "ANAME_17");
            DropColumn("dbo.STF02H", "ACODE_17");
            DropColumn("dbo.STF02H", "AVALUE_16");
            DropColumn("dbo.STF02H", "ANAME_16");
            DropColumn("dbo.STF02H", "ACODE_16");
            DropColumn("dbo.STF02H", "AVALUE_15");
            DropColumn("dbo.STF02H", "ANAME_15");
            DropColumn("dbo.STF02H", "ACODE_15");
            DropColumn("dbo.STF02H", "AVALUE_14");
            DropColumn("dbo.STF02H", "ANAME_14");
            DropColumn("dbo.STF02H", "ACODE_14");
            DropColumn("dbo.STF02H", "AVALUE_13");
            DropColumn("dbo.STF02H", "ANAME_13");
            DropColumn("dbo.STF02H", "ACODE_13");
            DropColumn("dbo.STF02H", "AVALUE_12");
            DropColumn("dbo.STF02H", "ANAME_12");
            DropColumn("dbo.STF02H", "ACODE_12");
            DropColumn("dbo.STF02H", "AVALUE_11");
            DropColumn("dbo.STF02H", "ANAME_11");
            DropColumn("dbo.STF02H", "ACODE_11");
            DropColumn("dbo.STF02H", "AVALUE_10");
            DropColumn("dbo.STF02H", "ANAME_10");
            DropColumn("dbo.STF02H", "ACODE_10");
            DropColumn("dbo.STF02H", "AVALUE_9");
            DropColumn("dbo.STF02H", "ANAME_9");
            DropColumn("dbo.STF02H", "ACODE_9");
            DropColumn("dbo.STF02H", "AVALUE_8");
            DropColumn("dbo.STF02H", "ANAME_8");
            DropColumn("dbo.STF02H", "ACODE_8");
            DropColumn("dbo.STF02H", "AVALUE_7");
            DropColumn("dbo.STF02H", "ANAME_7");
            DropColumn("dbo.STF02H", "ACODE_7");
            DropColumn("dbo.STF02H", "AVALUE_6");
            DropColumn("dbo.STF02H", "ANAME_6");
            DropColumn("dbo.STF02H", "ACODE_6");
            DropColumn("dbo.STF02H", "AVALUE_5");
            DropColumn("dbo.STF02H", "ANAME_5");
            DropColumn("dbo.STF02H", "ACODE_5");
            DropColumn("dbo.STF02H", "AVALUE_4");
            DropColumn("dbo.STF02H", "ANAME_4");
            DropColumn("dbo.STF02H", "ACODE_4");
            DropColumn("dbo.STF02H", "AVALUE_3");
            DropColumn("dbo.STF02H", "ANAME_3");
            DropColumn("dbo.STF02H", "ACODE_3");
            DropColumn("dbo.STF02H", "AVALUE_2");
            DropColumn("dbo.STF02H", "ANAME_2");
            DropColumn("dbo.STF02H", "ACODE_2");
            DropColumn("dbo.STF02H", "AVALUE_1");
            DropColumn("dbo.STF02H", "ANAME_1");
            DropColumn("dbo.STF02H", "ACODE_1");
            DropColumn("dbo.STF02H", "CATEGORY_NAME");
            DropColumn("dbo.STF02H", "CATEGORY_CODE");
        }
    }
}
