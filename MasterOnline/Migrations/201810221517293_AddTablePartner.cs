namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTablePartner : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Partner",
                c => new
                    {
                        PartnerId = c.Long(nullable: false, identity: true),
                        Email = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 30),
                        NoHp = c.String(nullable: false, maxLength: 30),
                        PhotoKtpUrl = c.String(maxLength: 255),
                        Status = c.Boolean(nullable: false),
                        PhotoKtpBase64 = c.String(),
                    })
                .PrimaryKey(t => t.PartnerId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Partner");
        }
    }
}
