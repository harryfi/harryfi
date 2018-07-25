namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTglInputGlfrek : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GLFREK", "TglInput", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GLFREK", "TglInput", c => c.String(maxLength: 10));
        }
    }
}
