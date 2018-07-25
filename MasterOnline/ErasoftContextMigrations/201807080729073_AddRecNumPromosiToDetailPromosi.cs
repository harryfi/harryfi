namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRecNumPromosiToDetailPromosi : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DetailPromosis", "RecNumPromosi", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DetailPromosis", "RecNumPromosi");
        }
    }
}
