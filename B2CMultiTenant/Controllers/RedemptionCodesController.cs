using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using B2CMultiTenant;
using B2CMultiTenant.Models;
using System.Security.Claims;
using B2CMultiTenant.Utilities;

namespace B2CMultiTenant.Controllers
{
    [CustomAuthorize(Roles = "admin", TenantType = Constants.B2CTenantType)]
    public class RedemptionCodesController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: RedemptionCodes
        public async Task<ActionResult> Index()
        {
            var tenantId = ClaimsPrincipal.Current.Claims.FirstOrDefault(c => c.Type == Constants.TenantIdClaim).Value;
            var codes = await db.NewUserCodes.
                Where(c => ((c.TenantId == tenantId) && (DateTime.Compare(DateTime.UtcNow, c.ExpiresBy) < 0))).
                ToListAsync();
            return View(codes);
        }
        private Random rand = new Random();
        // GET: RedemptionCodes/Create
        public async Task<ActionResult> Create()
        {
            var tenantId = ClaimsPrincipal.Current.Claims.FirstOrDefault(c => c.Type == Constants.TenantIdClaim).Value;
            var code = rand.Next(1000000, 9999999).ToString();
            db.NewUserCodes.Add(new RedemptionCode { TenantId = tenantId, Code = code, Role = "user", ExpiresBy = DateTime.UtcNow.AddDays(1) });
            await db.SaveChangesAsync();
            return Redirect("Index");
        }

        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var tenantId = ClaimsPrincipal.Current.Claims.FirstOrDefault(c => c.Type == Constants.TenantIdClaim).Value;
            var code = db.NewUserCodes.
                FirstOrDefault(c => ((c.Code == id) && (c.TenantId == tenantId) && (DateTime.Compare(DateTime.UtcNow, c.ExpiresBy) < 0)));
            if (code == null)
            {
                return HttpNotFound();
            }
            return View(code);
        }

        // POST: RedemptionCodes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var tenantId = ClaimsPrincipal.Current.Claims.FirstOrDefault(c => c.Type == Constants.TenantIdClaim).Value;
            var code = db.NewUserCodes.
                First(c => ((c.Code == id) && (c.TenantId == tenantId) && (DateTime.Compare(DateTime.UtcNow, c.ExpiresBy) < 0)));
            db.NewUserCodes.Remove(code);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if UseSQL
                db.Dispose();
#endif
            }
            base.Dispose(disposing);
        }
    }
}
