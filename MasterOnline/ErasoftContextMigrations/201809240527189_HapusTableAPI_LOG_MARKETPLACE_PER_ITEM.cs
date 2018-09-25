namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HapusTableAPI_LOG_MARKETPLACE_PER_ITEM : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.API_LOG_MARKETPLACE_PER_ITEM");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.API_LOG_MARKETPLACE_PER_ITEM",
                c => new
                    {
                        IDMARKET = c.Int(nullable: false),
                        REQUEST_ATTRIBUTE_1 = c.String(nullable: false, maxLength: 128),
                        CUST = c.String(maxLength: 10),
                        REQUEST_ACTION = c.String(),
                        REQUEST_DATETIME = c.DateTime(nullable: false),
                        REQUEST_STATUS = c.String(maxLength: 20),
                        REQUEST_RESULT = c.String(),
                        REQUEST_EXCEPTION = c.String(),
                    })
                .PrimaryKey(t => new { t.IDMARKET, t.REQUEST_ATTRIBUTE_1 });
            
        }
    }
}
