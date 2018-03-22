using B2CMultiTenant.Models;
using B2CMultiTenant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace B2CMultiTenant.Controllers
{
    [HandleError]
    public class MyBaseController: Controller
    {
        public string TenantName { get; set; }

        public MyBaseController()
        {
            var tenantNameClaim = ClaimsPrincipal.Current.FindFirst(Constants.TenantNameClaim);
            TenantName = "--Home--";
            if (tenantNameClaim != null)
                TenantName = tenantNameClaim.Value;
            this.ViewData["TenantName"] = TenantName;
        }
    }
}