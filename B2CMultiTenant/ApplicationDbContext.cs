using B2CMultiTenant.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace B2CMultiTenant
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<RedemptionCode> NewUserCodes { get; set; }
    }
}