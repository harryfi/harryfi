namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeliveryTemplateElevenia : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DeliveryTemplateElevenias",
                c => new
                    {
                        Recnum = c.Int(nullable: false, identity: true),
                        KODE = c.String(maxLength: 10),
                        KETERANGAN = c.String(maxLength: 250),
                        RECNUM_ARF01 = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Recnum);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DeliveryTemplateElevenias");
        }
    }
}
