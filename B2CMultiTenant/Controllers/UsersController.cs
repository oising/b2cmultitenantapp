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
    public class UsersController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public async Task<ActionResult> Index()
        {
            var tenantId = ClaimsPrincipal.Current.Claims.FirstOrDefault(c => c.Type == Constants.TenantIdClaim).Value;
            int i = 0;
            var usersInRoles = db.UserRoles.Where(t => t.TenantId == tenantId);
            var users = new List<User>();
            foreach(var u in usersInRoles)
            {
                var user = new User { Role = u.Role };
                await GraphHelper.GetUserAttr(u.UserObjectId, user);
                user.Id = i++;
                users.Add(user);
            }
            return View(users);
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = new Models.User();
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,DisplayName,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                //db.Users.Add(user);
                //await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            await Task.FromResult(0);
            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = new Models.User(); // await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DisplayName,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                //db.Entry(user).State = EntityState.Modified;
                //await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = null; //  await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //User user = await db.Users.FindAsync(id);
            //db.Users.Remove(user);
            //await db.SaveChangesAsync();
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
    }
}
