namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterRecnumDelivery : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DELIVERY_PROVIDER_LAZADA", "RECNUM", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DELIVERY_PROVIDER_LAZADA", "RECNUM", c => c.Int(nullable: false));
        }
    }
}
