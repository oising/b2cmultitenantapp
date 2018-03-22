using B2CMultiTenant.Models;
using B2CMultiTenant.Utilities;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace B2CMultiTenant.Controllers
{
    [CustomAuthorize(TenantType = Constants.B2CTenantType)]
    public class UserSetupController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            var userId = ClaimsPrincipal.Current.Claims.First(c => c.Type == Constants.ObjectIdClaim).Value;
            var model = new RedemptionCode();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "Code")] RedemptionCode rcode) // may be other attrs for user setup
        {
            try
            {
                var userId = ClaimsPrincipal.Current.Claims.First(c => c.Type == Constants.ObjectIdClaim).Value;
                var code = db.NewUserCodes.
                    First(c => (c.Code == rcode.Code) && (DateTime.Compare(DateTime.UtcNow, rcode.ExpiresBy) > 0));


                db.UserRoles.Add(new UserRole { TenantId = code.TenantId, UserObjectId = userId, Role = code.Role });
                db.NewUserCodes.Remove(code);
                db.SaveChanges();

                var id = (ClaimsIdentity)(ClaimsPrincipal.Current.Identity);
                // Remove current claims
                foreach (var role in id.FindAll("role"))
                    id.RemoveClaim(role); // remove roles in previous default tenant
                var tenantIdClaim = id.FindFirst(Constants.TenantIdClaim);
                if (tenantIdClaim != null)
                {
                    id.RemoveClaim(tenantIdClaim);
                    var tenantNameClaim = id.FindFirst(Constants.TenantNameClaim);
                    id.RemoveClaim(tenantNameClaim);
                }

                // Replace with new claims
                var tenantName = db.Tenants.First(t => t.Id == code.TenantId).Name;
                id.AddClaim(new Claim(Constants.TenantIdClaim, code.TenantId));
                id.AddClaim(new Claim(Constants.TenantNameClaim, tenantName));
                id.AddClaim(new Claim("role", code.Role));
                Request.GetOwinContext().Authentication.SignIn(id);

                ViewBag.Message = "User created";
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return RedirectToAction("Index", "Business");
        }

        public void Edit()
        {
            if (Request.IsAuthenticated)
            {
                string callbackUrl = Url.Action("Index", "Home", routeValues: null, protocol: Request.Url.Scheme);
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties() { RedirectUri = callbackUrl }, Startup.ProfilePolicyId);
            }
        }
    }
}