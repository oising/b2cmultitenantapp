using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using B2CMultiTenant;
using B2CMultiTenant.Models;
using System.Threading.Tasks;

namespace B2CMultiTenant.Controllers
{
    // controller that handles the onboarding of new tenants and new individual users
    // operates by starting an OAuth2 request on behalf of the user
    // during that request, the user is asked whether he/she consent for the app to gain access to the specified directory permissions.    
    public class AADOnboardingController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        public ActionResult SignUp()
        {
            // generate a random value to identify the request
            string stateMarker = Guid.NewGuid().ToString();
            // store it in the temporary entry for the tenant, we'll use it later to assess if the request was originated from us
            // this is necessary if we want to prevent attackers from provisioning themselves to access our app without having gone through our onboarding process (e.g. payments, etc)
            var tenant = new Tenant()
            {
                Name = "<new>",
                Id = stateMarker,
                IssValue = stateMarker,
                Created = DateTime.Now
            };
            db.Tenants.Add(tenant);
            db.SaveChanges();

            var clientId = ConfigurationManager.AppSettings["aad:ClientId"];
            var respUrl = Request.Url.GetLeftPart(UriPartial.Authority).ToString() + "/aad/aadOnboarding/ProcessCode";

            string authorizationRequest = String.Format(
                "https://login.microsoftonline.com/common/oauth2/authorize?response_type=code&client_id={0}&resource={1}&redirect_uri={2}&state={3}&prompt={4}",
                 Uri.EscapeDataString(clientId),
                 Uri.EscapeDataString("https://graph.windows.net"),
                 Uri.EscapeDataString(respUrl),
                 Uri.EscapeDataString(stateMarker),
                 Uri.EscapeDataString("admin_consent")
                 );
            // send the user to consent
            return new RedirectResult(authorizationRequest);
        }

        // GET: /TOnboarding/ProcessCode
        public async Task<ActionResult> ProcessCode(string code, string error, string error_description, string resource, string state)
        {
            // Is this a response to a request we generated? Let's see if the state is carrying an ID we previously saved
            // ---if we don't, return an error            
            if (db.Tenants.FirstOrDefault(a => a.IssValue == state) == null)
            {
                // TODO: prettify
                throw new Exception("Unsolicited admin consent response");
            }
            else
            {
                // ---if the response is indeed from a request we generated
                // ------get a token for the Graph, that will provide us with information abut the caller
                ClientCredential credential = new ClientCredential(ConfigurationManager.AppSettings["aad:ClientID"],
                                                                   ConfigurationManager.AppSettings["aad:ClientSecret"]);
                AuthenticationContext authContext = new AuthenticationContext("https://login.microsoftonline.com/common/");
                AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(
                    code, new Uri(Request.Url.GetLeftPart(UriPartial.Path)), credential);

                var myTenant = db.Tenants.FirstOrDefault(a => a.IssValue == state);
                myTenant.AdminConsented = true;
                string issuer = String.Format("https://sts.windows.net/{0}/", result.TenantId);
                myTenant.IssValue = issuer;

                // remove older, unclaimed entries
                DateTime tenMinsAgo = DateTime.Now.Subtract(new TimeSpan(0, 10, 0)); // workaround for Linq to entities
                var garbage = db.Tenants.Where(a => (!a.IssValue.StartsWith("https") && (a.Created < tenMinsAgo)));
                foreach (Tenant t in garbage)
                    db.Tenants.Remove(t);

                db.SaveChanges();
                // ------return a view claiming success, inviting the user to sign in
                return View();
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}