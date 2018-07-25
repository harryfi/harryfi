namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddColumnAndAddTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DetailPromosis",
                c => new
                    {
                        RecNum = c.Int(nullable: false, identity: true),
                        KODE_BRG = c.String(maxLength: 20),
                        HARGA_NORMAL = c.Double(nullable: false),
                        HARGA_PROMOSI = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.RecNum);
            
            CreateTable(
                "dbo.Promosis",
                c => new
                    {
                        RecNum = c.Int(nullable: false, identity: true),
                        NAMA_PROMOSI = c.String(maxLength: 50),
                        NAMA_MARKET = c.String(maxLength: 30),
                        TGL_MULAI = c.DateTime(storeType: "date"),
                        TGL_AKHIR = c.DateTime(storeType: "date"),
                    })
                .PrimaryKey(t => t.RecNum);
            
            AddColumn("dbo.STF02", "DISPLAY_MARKET", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.STF02", "DISPLAY_MARKET");
            DropTable("dbo.Promosis");
            DropTable("dbo.DetailPromosis");
        }
    }
}
