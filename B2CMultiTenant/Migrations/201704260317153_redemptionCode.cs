namespace B2CMultiTenant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class redemptionCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RedemptionCodes", "ExpiresBy", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RedemptionCodes", "ExpiresBy");
        }
    }
}
