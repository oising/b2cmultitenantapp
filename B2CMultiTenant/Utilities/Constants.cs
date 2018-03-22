using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2CMultiTenant.Utilities
{
    public static class Constants
    {
        public static readonly string AAD_ClassicAuthn = "AAD-MT";
        public static readonly string TenantIdClaim = "TenantId";
        public static readonly string TenantNameClaim = "TenantName";
        public static readonly string ObjectIdClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        public static readonly string PolicyIdClaim = "tfp";
        public static readonly string RoleClaim = "role";
        public static readonly string TenantTypeClaim = "tenantType"; // B2C or AADMT

        public static readonly string Admin = "admin";
        public static readonly string User = "user";

        public const string B2CTenantType = "B2C";
        public const string AADClassicTenantType = "AADMT";

    }
}