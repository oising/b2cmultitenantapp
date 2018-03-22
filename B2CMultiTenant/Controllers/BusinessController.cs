using B2CMultiTenant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace B2CMultiTenant.Controllers
{
    [CustomAuthorize(Roles = "admin,user")]
    public class BusinessController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Business
        public ActionResult Index()
        {
            var tenantId = (ClaimsPrincipal.Current.FindFirst(c => c.Type == Constants.TenantIdClaim)).Value;
            ViewBag.TenantName = db.Tenants.First(t => t.Id == tenantId).Name;
            return View();
        }
    }
}