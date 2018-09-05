namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class api_log_marketplace_view_dan_LOG_REQUEST_ID_di_queueFeedBlibli : DbMigration
    {
        public override void Up()
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
                    })
                .PrimaryKey(t => new { t.IDMARKET, t.REQUEST_ATTRIBUTE_1 });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.API_LOG_MARKETPLACE_PER_ITEM");
        }
    }
}
