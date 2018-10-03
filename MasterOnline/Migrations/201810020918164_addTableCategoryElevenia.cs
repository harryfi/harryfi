namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTableCategoryElevenia : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CATEGORY_ELEVENIA",
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
            DropTable("dbo.CATEGORY_ELEVENIA");
        }
    }
}
