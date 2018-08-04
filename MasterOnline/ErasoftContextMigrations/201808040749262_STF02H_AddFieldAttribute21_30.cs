namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class STF02H_AddFieldAttribute21_30 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.STF02H", "ACODE_21", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_21", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_21", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_22", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_22", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_22", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_23", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_23", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_23", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_24", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_24", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_24", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_25", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_25", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_25", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_26", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_26", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_26", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_27", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_27", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_27", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_28", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_28", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_28", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_29", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_29", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_29", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "ACODE_30", c => c.String(maxLength: 50));
            AddColumn("dbo.STF02H", "ANAME_30", c => c.String(maxLength: 250));
            AddColumn("dbo.STF02H", "AVALUE_30", c => c.String(maxLength: 250));
        }
        
        public override void Down()
        {
            DropColumn("dbo.STF02H", "AVALUE_30");
            DropColumn("dbo.STF02H", "ANAME_30");
            DropColumn("dbo.STF02H", "ACODE_30");
            DropColumn("dbo.STF02H", "AVALUE_29");
            DropColumn("dbo.STF02H", "ANAME_29");
            DropColumn("dbo.STF02H", "ACODE_29");
            DropColumn("dbo.STF02H", "AVALUE_28");
            DropColumn("dbo.STF02H", "ANAME_28");
            DropColumn("dbo.STF02H", "ACODE_28");
            DropColumn("dbo.STF02H", "AVALUE_27");
            DropColumn("dbo.STF02H", "ANAME_27");
            DropColumn("dbo.STF02H", "ACODE_27");
            DropColumn("dbo.STF02H", "AVALUE_26");
            DropColumn("dbo.STF02H", "ANAME_26");
            DropColumn("dbo.STF02H", "ACODE_26");
            DropColumn("dbo.STF02H", "AVALUE_25");
            DropColumn("dbo.STF02H", "ANAME_25");
            DropColumn("dbo.STF02H", "ACODE_25");
            DropColumn("dbo.STF02H", "AVALUE_24");
            DropColumn("dbo.STF02H", "ANAME_24");
            DropColumn("dbo.STF02H", "ACODE_24");
            DropColumn("dbo.STF02H", "AVALUE_23");
            DropColumn("dbo.STF02H", "ANAME_23");
            DropColumn("dbo.STF02H", "ACODE_23");
            DropColumn("dbo.STF02H", "AVALUE_22");
            DropColumn("dbo.STF02H", "ANAME_22");
            DropColumn("dbo.STF02H", "ACODE_22");
            DropColumn("dbo.STF02H", "AVALUE_21");
            DropColumn("dbo.STF02H", "ANAME_21");
            DropColumn("dbo.STF02H", "ACODE_21");
        }
    }
}
