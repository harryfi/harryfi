namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTableForm : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FormMos",
                c => new
                    {
                        ScrId = c.Int(nullable: false, identity: true),
                        ParentId = c.Int(nullable: false),
                        NamaForm = c.String(nullable: false, maxLength: 30),
                        Url = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.ScrId, clustered: false);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FormMos");
        }
    }
}
