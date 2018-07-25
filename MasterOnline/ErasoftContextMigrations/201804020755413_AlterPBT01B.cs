namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterPBT01B : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PBT01B", "NO", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PBT01B", "NO", c => c.Int(nullable: false));
        }
    }
}
