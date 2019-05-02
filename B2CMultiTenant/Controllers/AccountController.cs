using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;
using System.Security.Claims;
using B2CMultiTenant.Utilities;

namespace B2CMultiTenant.Controllers
{
    public class AccountController : MyBaseController
    {
        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                var authType = ((Request.Url.Segments.Length > 1) && (Request.Url.Segments[1].ToLower() == "aad/")) ?
                    Constants.AAD_ClassicAuth : Startup.SignUpInPolicyId;
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties() { RedirectUri = "/" }, authType);
            }
        }
        public ActionResult SignInCreator()
        {
            if (!Request.IsAuthenticated)
            {
                if ((Request.Url.Segments.Length > 1) && (Request.Url.Segments[1].ToLower() == "aad/"))
                    return RedirectToAction("SignUp", "AADOnboarding");
                else
                    HttpContext.GetOwinContext().Authentication.Challenge(
                        new AuthenticationProperties() { RedirectUri = "b2c/Tenants/Create" }, Startup.SignUpInPolicyId);
                return null;
            }
            return View();
        }
        [Authorize]
        public void SignOut()
        {
            string callbackUrl = Url.Action("SignOutCallback", "Account", routeValues: null, protocol: Request.Url.Scheme);

            var tenantType = ClaimsPrincipal.Current.FindFirst(Constants.TenantTypeClaim).Value;
            var authType = (tenantType == Constants.AADClassicTenantType) ? Constants.AADClassicTenantType : Startup.SignUpInPolicyId;
            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                authType, CookieAuthenticationDefaults.AuthenticationType);
        }
        public ActionResult SignOutCallback()
        {
            return View();
        }

        public void ResetPassword()
        {
            // Let the middleware know you are trying to use the reset password policy (see OnRedirectToIdentityProvider in Startup.Auth.cs)
            HttpContext.GetOwinContext().Set("Policy", Startup.ResetPwdPolicyId);

            // Set the page to redirect to after changing passwords
            var authenticationProperties = new AuthenticationProperties { RedirectUri = "/" };
            HttpContext.GetOwinContext().Authentication.Challenge(authenticationProperties);

            return;
        }
    }
}
