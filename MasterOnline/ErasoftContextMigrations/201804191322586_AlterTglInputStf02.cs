namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTglInputStf02 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.STF02", "Tgl_Input", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.STF02", "Tgl_Input", c => c.DateTime(nullable: false));
        }
    }
}
