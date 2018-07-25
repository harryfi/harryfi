namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTableSifsysTambahan : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SIFSYS_TAMBAHAN",
                c => new
                    {
                        RecNum = c.Int(nullable: false, identity: true),
                        PERSON = c.String(maxLength: 30),
                        EMAIL = c.String(nullable: false, maxLength: 30),
                        TELEPON = c.String(nullable: false, maxLength: 30),
                        KODEPOS = c.String(nullable: false, maxLength: 7),
                        KODEKABKOT = c.String(nullable: false, maxLength: 4),
                        KODEPROV = c.String(nullable: false, maxLength: 2),
                        NAMA_KABKOT = c.String(maxLength: 50),
                        NAMA_PROV = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.RecNum);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SIFSYS_TAMBAHAN");
        }
    }
}
