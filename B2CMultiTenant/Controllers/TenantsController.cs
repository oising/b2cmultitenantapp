using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using B2CMultiTenant.Models;
using System.Security.Claims;
using B2CMultiTenant.Utilities;

namespace B2CMultiTenant.Controllers
{
    [CustomAuthorize(TenantType = Constants.B2CTenantType)]
    public class TenantsController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tenants
        public async Task<ActionResult> Index()
        {
            var tenants = await GetUserTenants();
            return View(tenants);
        }

        // GET: Tenants/Details/5
        public async Task<ActionResult> Select(int id)
        {
            var tenants = await GetUserTenants();
            var tenantName = tenants[id];
            var tenant = db.Tenants.First(t => t.Name == tenantName);
            var roles = await db.UserRoles
                .Where(r => r.TenantId == tenant.Id)
                .Select(r => r.Role)
                .ToListAsync();
            SetUserTenant(tenant.Id, roles);
            return RedirectToAction("Index", "Home");
        }

        // GET: Tenants/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tenants/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name")] Tenant tenant)
        {
            // Validate name syntax; use Regex
            tenant.Name = tenant.Name.Trim(' ', '.', ';');
            try
            {
                var existingTenant = db.Tenants.FirstOrDefault(t => t.Name == tenant.Name);
                if (existingTenant != null)
                    throw new Exception("A tenant with same name already exists");
                tenant.Id = Guid.NewGuid().ToString();
                tenant.Created = DateTime.UtcNow;
                tenant.AdminConsented = true;
                tenant.IssValue = string.Empty;
                db.Tenants.Add(tenant);

                var userObjId = ClaimsPrincipal.Current.Claims.First(c => c.Type == Constants.ObjectIdClaim).Value;
                db.UserRoles.Add(new UserRole { TenantId = tenant.Id, UserObjectId = userObjId, Role = Constants.Admin });
                db.UserRoles.Add(new UserRole { TenantId = tenant.Id, UserObjectId = userObjId, Role = Constants.User });

                db.SaveChanges();
                SetUserTenant(tenant.Id, new string[] { Constants.Admin, Constants.User });
                ViewBag.Message = "New tenant was created successfully.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private async Task<Dictionary<int, string>> GetUserTenants()
        {
            var userId = ClaimsPrincipal.Current.FindFirst(c => c.Type == Constants.ObjectIdClaim).Value;
            var i = 0;
            var tenants = await db.UserRoles
                .Where(r => r.UserObjectId == userId)
                .Select(r => r.TenantId)
                .Distinct()
                .Join(db.Tenants, r => r, t => t.Id, (r, t) => t.Name)
                .ToDictionaryAsync(t => i++);
            return tenants;
        }
        private void SetUserTenant(string tenantId, IEnumerable<string> roles)
        {
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
            var tenantName = db.Tenants.First(t => t.Id == tenantId).Name;
            id.AddClaim(new Claim(Constants.TenantIdClaim, tenantId));
            id.AddClaim(new Claim(Constants.TenantNameClaim, tenantName));
            foreach (var r in roles)
                id.AddClaim(new Claim("role", r));
            Request.GetOwinContext().Authentication.SignIn(id);
        }
    }
}
