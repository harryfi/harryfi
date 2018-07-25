namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterDateTimeTransaksi : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PBT01A", "TGL", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.PBT01A", "TGLINPUT", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.PBT01B", "TGLINPUT", c => c.DateTime(nullable: false, storeType: "date"));
            AlterColumn("dbo.SIT01A", "TGL", c => c.DateTime(nullable: false, storeType: "date"));
            AlterColumn("dbo.SIT01A", "TGLINPUT", c => c.DateTime(nullable: false, storeType: "date"));
            AlterColumn("dbo.SIT01B", "TGLINPUT", c => c.DateTime(storeType: "date"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SIT01B", "TGLINPUT", c => c.DateTime());
            AlterColumn("dbo.SIT01A", "TGLINPUT", c => c.DateTime(nullable: false));
            AlterColumn("dbo.SIT01A", "TGL", c => c.DateTime(nullable: false));
            AlterColumn("dbo.PBT01B", "TGLINPUT", c => c.DateTime(nullable: false));
            AlterColumn("dbo.PBT01A", "TGLINPUT", c => c.DateTime());
            AlterColumn("dbo.PBT01A", "TGL", c => c.DateTime());
        }
    }
}
