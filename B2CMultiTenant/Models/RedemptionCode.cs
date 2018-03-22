using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B2CMultiTenant.Models
{
    public partial class RedemptionCode
    {
        [Key]
        public string Code { get; set; }
        public string TenantId { get; set; }
        public string Role { get; set; }
        public System.DateTime ExpiresBy { get; set; }
    }
}
