namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFieldToPromosiAndDetailPromosi : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DetailPromosis", "TGL_INPUT", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.Promosis", "TGL_INPUT", c => c.DateTime(storeType: "date"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Promosis", "TGL_INPUT");
            DropColumn("dbo.DetailPromosis", "TGL_INPUT");
        }
    }
}
