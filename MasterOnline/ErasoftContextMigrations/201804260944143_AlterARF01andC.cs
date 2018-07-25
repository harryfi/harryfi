namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterARF01andC : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ARF01", "TOKEN", c => c.String(maxLength: 50));
            AddColumn("dbo.ARF01C", "NAMA_KABKOT", c => c.String(maxLength: 50));
            AddColumn("dbo.ARF01C", "NAMA_PROV", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ARF01C", "NAMA_PROV");
            DropColumn("dbo.ARF01C", "NAMA_KABKOT");
            DropColumn("dbo.ARF01", "TOKEN");
        }
    }
}
