namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProvinsiKabkot : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KabupatenKota",
                c => new
                    {
                        KodeKabKot = c.String(nullable: false, maxLength: 4),
                        KodeProv = c.String(nullable: false, maxLength: 2),
                        NamaKabKot = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.KodeKabKot);
            
            CreateTable(
                "dbo.Provinsi",
                c => new
                    {
                        KodeProv = c.String(nullable: false, maxLength: 2),
                        NamaProv = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.KodeProv);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Provinsi");
            DropTable("dbo.KabupatenKota");
        }
    }
}
