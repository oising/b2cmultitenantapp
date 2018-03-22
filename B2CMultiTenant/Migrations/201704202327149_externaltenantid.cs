namespace B2CMultiTenant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class externaltenantid : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.UserRoles");
            CreateTable(
                "dbo.RedemptionCodes",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                        TenantId = c.Int(nullable: false),
                        Role = c.String(),
                    })
                .PrimaryKey(t => t.Code);
            
            AlterColumn("dbo.UserRoles", "TenantId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.UserRoles", "UserObjectId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.UserRoles", new[] { "TenantId", "UserObjectId" });
            DropColumn("dbo.UserRoles", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserRoles", "Id", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.UserRoles");
            AlterColumn("dbo.UserRoles", "UserObjectId", c => c.String());
            AlterColumn("dbo.UserRoles", "TenantId", c => c.String());
            DropTable("dbo.RedemptionCodes");
            AddPrimaryKey("dbo.UserRoles", "Id");
        }
    }
}
