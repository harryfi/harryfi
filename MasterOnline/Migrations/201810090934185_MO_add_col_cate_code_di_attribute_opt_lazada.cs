namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MO_add_col_cate_code_di_attribute_opt_lazada : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ATTRIBUTE_OPT_LAZADA", "CATEGORY_CODE", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ATTRIBUTE_OPT_LAZADA", "CATEGORY_CODE");
        }
    }
}
