using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2CMultiTenant.Models
{
    public class UserTenantSelection
    {
        public int Selection { get; set; }
        public Dictionary<int,string> Tenants { get; set; }
    }
}