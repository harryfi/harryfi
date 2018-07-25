namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterKeyPromo : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Promo");
            AlterColumn("dbo.Promo", "EMAIL_REF", c => c.String(maxLength: 50));
            AddPrimaryKey("dbo.Promo", "RecNum");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Promo");
            AlterColumn("dbo.Promo", "EMAIL_REF", c => c.String(nullable: false, maxLength: 50));
            AddPrimaryKey("dbo.Promo", "EMAIL_REF");
        }
    }
}
