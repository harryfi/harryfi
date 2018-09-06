namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterFieldEmailLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ARF01", "EMAIL", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.ARF01C", "EMAIL", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.SIF12", "EMAIL", c => c.String(maxLength: 50, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SIF12", "EMAIL", c => c.String(maxLength: 30, unicode: false));
            AlterColumn("dbo.ARF01C", "EMAIL", c => c.String(nullable: false, maxLength: 30));
            AlterColumn("dbo.ARF01", "EMAIL", c => c.String(nullable: false, maxLength: 30));
        }
    }
}
