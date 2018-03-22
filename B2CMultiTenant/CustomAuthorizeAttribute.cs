using B2CMultiTenant.Utilities;
using System;
using System.Linq;
using System.Web;

namespace B2CMultiTenant
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class CustomAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        public string TenantType { get; set; }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var tenantType = System.Security.Claims.ClaimsPrincipal.Current.FindFirst(Constants.TenantTypeClaim);
            if (!String.IsNullOrEmpty(TenantType) && (TenantType != tenantType.Value))
                return false;
            return base.AuthorizeCore(httpContext);
        }
        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            //var claims = System.Security.Claims.ClaimsPrincipal.Current.Claims.ToList();
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new System.Web.Mvc.HttpStatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }

}