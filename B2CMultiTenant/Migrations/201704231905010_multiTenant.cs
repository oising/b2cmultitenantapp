namespace B2CMultiTenant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class multiTenant : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RedemptionCodes", "TenantId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RedemptionCodes", "TenantId", c => c.Int(nullable: false));
        }
    }
}
