namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class API_LOG_MARKETPLACE : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.API_LOG_MARKETPLACE",
                c => new
                    {
                        RECNUM = c.Int(nullable: false, identity: true),
                        CUST = c.String(maxLength: 10),
                        CUST_ATTRIBUTE_1 = c.String(maxLength: 100),
                        CUST_ATTRIBUTE_2 = c.String(maxLength: 100),
                        CUST_ATTRIBUTE_3 = c.String(maxLength: 100),
                        CUST_ATTRIBUTE_4 = c.String(maxLength: 100),
                        CUST_ATTRIBUTE_5 = c.String(maxLength: 100),
                        MARKETPLACE = c.String(maxLength: 100),
                        REQUEST_ID = c.String(),
                        REQUEST_ACTION = c.String(),
                        REQUEST_DATETIME = c.DateTime(nullable: false),
                        REQUEST_STATUS = c.String(maxLength: 20),
                        REQUEST_ATTRIBUTE_1 = c.String(),
                        REQUEST_ATTRIBUTE_2 = c.String(),
                        REQUEST_ATTRIBUTE_3 = c.String(),
                        REQUEST_ATTRIBUTE_4 = c.String(),
                        REQUEST_ATTRIBUTE_5 = c.String(),
                        REQUEST_RESULT = c.String(),
                        REQUEST_EXCEPTION = c.String(),
                    })
                .PrimaryKey(t => t.RECNUM);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.API_LOG_MARKETPLACE");
        }
    }
}
