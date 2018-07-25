namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterALAPF01Length : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.APF01", "AL", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.APF01", "AL", c => c.String(maxLength: 30));
        }
    }
}
