using B2CMultiTenant.Models;
using B2CMultiTenant.Utilities;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace B2CMultiTenant
{
    public static class GraphHelper
    {
        public static async Task GetUserAttr(string userId, User user)
        {
            var tenantId = ClaimsPrincipal.Current.Claims.FirstOrDefault(c => c.Type == Constants.TenantIdClaim)?.Value;
            var ctx = new AuthenticationContext("https://login.microsoftonline.com/b2cmultitenant.onmicrosoft.com/");
            var resp = ctx.AcquireTokenAsync("https://graph.windows.net",
                new ClientCredential(
                    "b2672267-bc35-4e97-b36a-913bffed4643",
                    "VWGLdG+YTjzCksOdCw9xls5/oPlUNuiDnaiEa0oa/P4=")).Result;
            var http = new HttpClient();
            var requestUri = $"https://graph.windows.net/b2cmultitenant.onmicrosoft.com/users/{userId}?api-version=1.6";
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resp.AccessToken);
            var usersJson = await http.GetStringAsync(requestUri);
            user.DisplayName = (string) JObject.Parse(usersJson)["displayName"];
        }

        private static async Task<bool> UpdateCustomAttr(string tenantId, string userObjId)
        {
            //var users = http.GetStringAsync(String.Format("https://graph.windows.net/b2cmultitenant.onmicrosoft.com/users/{0}?api-version=1.6", userObjId)).Result;
            //var users = http.GetStringAsync(String.Format("https://graph.windows.net/b2cmultitenant.onmicrosoft.com/applications/{0}/extensionProperties?api-version=1.6", extApp)).Result;

            var ctx = new AuthenticationContext("https://login.microsoftonline.com/b2cmultitenant.onmicrosoft.com/");
            var resp = ctx.AcquireTokenAsync("https://graph.windows.net",
                new ClientCredential(
                    "b2672267-bc35-4e97-b36a-913bffed4643",
                    "VWGLdG+YTjzCksOdCw9xls5/oPlUNuiDnaiEa0oa/P4=")).Result;

            // use https://graph.windows.net/myorganization/applications/20d75341-a1e5-4ea8-a88f-7e7dfe90b9d8/extensionProperties to get all properties
            // extension_0428f3354957491e96bb7ce51b81d46a_TenantId
            //var extApp = "20d75341-a1e5-4ea8-a88f-7e7dfe90b9d8"; // - graph extensions

            var http = new HttpClient();
            var requestUri = String.Format("https://graph.windows.net/b2cmultitenant.onmicrosoft.com/users/{0}?api-version=1.6", userObjId);
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resp.AccessToken);
            var extProp = new
            {
                extension_0428f3354957491e96bb7ce51b81d46a_TenantId = tenantId
            };
            var content = JsonConvert.SerializeObject(extProp);
            //var content = String.Format("{{\"extension_0428f3354957491e96bb7ce51b81d46a_TenantId\": \"{0}\"}", tenant.Id);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            { Content = new StringContent(content, System.Text.Encoding.ASCII, "application/json") };
            var httpResp = await http.SendAsync(request);
            return (httpResp.IsSuccessStatusCode);
        }
    }
}