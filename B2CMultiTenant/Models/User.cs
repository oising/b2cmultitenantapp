using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2CMultiTenant.Models
{
    // For view display only. Not a DB table
    public class User
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
    }
}