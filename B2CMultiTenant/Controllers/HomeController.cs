using B2CMultiTenant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace B2CMultiTenant.Controllers
{
    public class HomeController : MyBaseController
    {
        // GET: Home
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                if (ClaimsPrincipal.Current.HasClaim(c => c.Type == Constants.TenantIdClaim) ||
                    (ClaimsPrincipal.Current.FindFirst(Constants.TenantTypeClaim).Value == Constants.AADClassicTenantType))
                    return RedirectToAction("Index", "Business");
                else
                    return RedirectToAction("Index", "UserSetup");
            }
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
    }
}