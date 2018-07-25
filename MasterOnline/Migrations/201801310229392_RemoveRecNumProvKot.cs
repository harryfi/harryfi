namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRecNumProvKot : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.KabupatenKota", "RecNum");
            DropColumn("dbo.Provinsi", "RecNum");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Provinsi", "RecNum", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.KabupatenKota", "RecNum", c => c.Int(nullable: false, identity: true));
        }
    }
}
