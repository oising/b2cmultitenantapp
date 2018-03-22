using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Notifications;
using Microsoft.IdentityModel.Protocols;
using System.IdentityModel.Tokens;
using B2CMultiTenant.Utilities;

namespace B2CMultiTenant
{
    public partial class Startup
    {
        // App config settings
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];

        //private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

        // B2C policy identifiers
        //public static string SignUpInPolicyId = "B2C_1A_MTSUSI";
        public static string SignUpInPolicyId = "B2C_1A_MTSUSI";
        public static string ProfilePolicyId = "B2C_1_MTEditProfile";
        public static string ResetPwdPolicyId = "B2C_1_ResetPwd";

        private ApplicationDbContext db = new ApplicationDbContext();

        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseErrorPage();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            // Configure OpenID Connect middleware for each policy
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(Startup.ProfilePolicyId));
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(Startup.SignUpInPolicyId));
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(Startup.ResetPwdPolicyId));

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    AuthenticationType = Constants.AAD_ClassicAuthn,
                    ClientId = ConfigurationManager.AppSettings["aad:ClientId"],
                    Authority = "https://login.microsoftonline.com/common/",
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        RedirectToIdentityProvider = (context) =>
                        {
                            // This ensures that the address used for sign in and sign out is picked up dynamically from the request
                            // this allows you to deploy your app (to Azure Web Sites, for example)without having to change settings
                            // Remember that the base URL of the address used here must be provisioned in Azure AD beforehand.
                            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                            context.ProtocolMessage.RedirectUri = appBaseUrl;
                            context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                            return Task.FromResult(0);
                        },
                        // we use this notification for injecting our custom logic
                        SecurityTokenValidated = (context) =>
                        {
                            // retriever caller data from the incoming principal
                            string issuer = context.AuthenticationTicket.Identity.FindFirst("iss").Value;
                            string UPN = context.AuthenticationTicket.Identity.FindFirst(System.Security.Claims.ClaimTypes.Name).Value;
                            string tenantID = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;

                            if (
                                // the caller comes from an admin-consented, recorded issuer
                                (db.Tenants.FirstOrDefault(a => ((a.Id == tenantID) && (a.AdminConsented))) == null)
                                // the caller is recorded in the db of users who went through the individual onboardoing
                                //&& (db.Users.FirstOrDefault(b => ((b.UPN == UPN) && (b.TenantID == tenantID))) == null)
                                )
                                // the caller was neither from a trusted issuer or a registered user - throw to block the authentication flow
                                throw new SecurityTokenValidationException();
                            context.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(Constants.TenantIdClaim, tenantID));
                            context.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(Constants.TenantNameClaim, UPN.Split('@')[1]));
                            context.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(Constants.TenantTypeClaim, Constants.AADClassicTenantType));
                            return Task.FromResult(0);
                        },
                        AuthenticationFailed = AuthenticationFailed,
                    }
                });
        }
        private OpenIdConnectAuthenticationOptions CreateOptionsFromPolicy(string policy)
        {
            var resp = new OpenIdConnectAuthenticationOptions
            {
                MetadataAddress = String.Format(aadInstance, tenant, policy),
                AuthenticationType = policy,

                ClientId = clientId,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = SetResponseUrls,
                    AuthenticationFailed = AuthenticationFailed,
                    SecurityTokenValidated = async (ctx) =>
                    {
                        try
                        {
                            ctx.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(Constants.TenantTypeClaim, Constants.B2CTenantType));
                            var userId = ctx.AuthenticationTicket.Identity.Claims.First(c => c.Type == Constants.ObjectIdClaim).Value;
                            var roles = db.UserRoles.Where(r => r.UserObjectId == userId);
                            if (roles.Count() > 0)
                            {
                                //TODO: use a persistent cookie to hold unto user tenant preference for future sessions
                                var tenantId = roles.First().TenantId;
                                var tenantName = db.Tenants.First(t => t.Id == tenantId).Name;
                                ctx.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(Constants.TenantIdClaim, tenantId));
                                ctx.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(Constants.TenantNameClaim, tenantName));
                                foreach (var r in roles.Where(role => role.TenantId == tenantId))
                                    ctx.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(Constants.RoleClaim, r.Role));
                            }
                            await Task.FromResult(0);
                        } catch(Exception ex)
                        {
                            throw new SecurityTokenValidationException();
                        }
                    },
                },
                Scope = "openid",
                ResponseType = "id_token",
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role",
                },
            };
            return resp;
        }
        private async Task SetResponseUrls(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> ctx)
        {
            var uri = ctx.Request.Uri;
            var url = String.Format("{0}://{1}/", uri.Scheme, uri.Authority);
            ctx.ProtocolMessage.RedirectUri = url;
            ctx.ProtocolMessage.PostLogoutRedirectUri = url;
            await Task.FromResult(0);
        }
        // Used for avoiding yellow-screen-of-death
        private Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();

            // Handle the error code that Azure AD B2C throws when trying to reset a password from the login page 
            // because password reset is not supported by a "sign-up or sign-in policy"
            if (notification.ProtocolMessage.ErrorDescription != null && notification.ProtocolMessage.ErrorDescription.Contains("AADB2C90118"))
            {
                // If the user clicked the reset password link, redirect to the reset password route
                notification.Response.Redirect("/Account/ResetPassword");
            }
            else if (notification.Exception.Message == "access_denied")
            {
                notification.Response.Redirect("/");
            }
            else
            {
                notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
            }
            return Task.FromResult(0);
        }

    }
}
