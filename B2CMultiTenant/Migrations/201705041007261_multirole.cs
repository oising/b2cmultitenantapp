namespace B2CMultiTenant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class multirole : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.UserRoles");
            AlterColumn("dbo.UserRoles", "Role", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.UserRoles", new[] { "TenantId", "UserObjectId", "Role" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.UserRoles");
            AlterColumn("dbo.UserRoles", "Role", c => c.String());
            AddPrimaryKey("dbo.UserRoles", new[] { "TenantId", "UserObjectId" });
        }
    }
}
