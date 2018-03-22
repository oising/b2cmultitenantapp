using System;
using System.Collections.Generic;

namespace B2CMultiTenant.Models
{
    public partial class Tenant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool AdminConsented { get; set; }
        public string IssValue { get; set; }
        public DateTime Created { get; set; }
    }
}
