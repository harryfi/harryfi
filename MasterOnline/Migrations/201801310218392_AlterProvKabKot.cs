namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterProvKabKot : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KabupatenKota", "RecNum", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.Provinsi", "RecNum", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Provinsi", "RecNum");
            DropColumn("dbo.KabupatenKota", "RecNum");
        }
    }
}
