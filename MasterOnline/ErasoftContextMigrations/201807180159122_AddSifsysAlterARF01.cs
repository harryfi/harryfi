namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSifsysAlterARF01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SIFSYS", "CORPORATE_ID", c => c.String(maxLength: 50));
            AddColumn("dbo.SIFSYS", "REKBCA", c => c.String(maxLength: 50));
            AddColumn("dbo.SIFSYS", "NAMAREKBCA", c => c.String(maxLength: 50));
            AlterColumn("dbo.ARF01", "TOKEN", c => c.String(maxLength: 100));
            AlterColumn("dbo.ARF01", "REFRESH_TOKEN", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ARF01", "REFRESH_TOKEN", c => c.String(maxLength: 50));
            AlterColumn("dbo.ARF01", "TOKEN", c => c.String(maxLength: 50));
            DropColumn("dbo.SIFSYS", "NAMAREKBCA");
            DropColumn("dbo.SIFSYS", "REKBCA");
            DropColumn("dbo.SIFSYS", "CORPORATE_ID");
        }
    }
}
