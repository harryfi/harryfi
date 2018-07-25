namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add2FieldInSot01b : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SOT01B", "ORDER_ITEM_ID", c => c.String(maxLength: 50));
            AddColumn("dbo.SOT01B", "STATUS_BRG", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SOT01B", "STATUS_BRG");
            DropColumn("dbo.SOT01B", "ORDER_ITEM_ID");
        }
    }
}
