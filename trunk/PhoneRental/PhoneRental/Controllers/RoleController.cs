using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhoneRental.Models;
using System.Web.Security;
using WebMatrix.WebData;

namespace PhoneRental.Controllers
{
    [Authorize(Roles="Admin")]
    public class RoleController : Controller
    {
        private PhoneRentalContext db = new PhoneRentalContext();

        //
        // GET: /Role/

        public ActionResult Index()
        {
            var users = db.UserProfiles.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ThenBy(u => u.UserName).ToList();

            List<RoleModel> rm = new List<RoleModel>();
            foreach (var user in users) {
                RoleModel roleModel = new RoleModel(user);
                roleModel.Role = "Ügyfél";

                if (Roles.IsUserInRole(user.UserName, "Admin"))
                {
                    roleModel.Role = "Adminisztrátor";
                }

                roleModel.Avatar = AccountController.GetMD5HashData(user.UserName);

                rm.Add(roleModel);
            }

            return View(rm);
        }

        //
        // GET: /Role/Edit/5

        public ActionResult Edit(int id = 0)
        {
            UserProfile userprofile = db.UserProfiles.Find(id);
            if (userprofile == null)
            {
                return HttpNotFound();
            }

            Boolean isAdmin = false;
            if (Roles.IsUserInRole(userprofile.UserName, "Admin"))
            {
                isAdmin = true;
            }

            RoleModel model = new RoleModel(userprofile);
            model.IsAdmin = isAdmin;

            return View(model);
        }

        //
        // POST: /Role/Edit/5

        [HttpPost]
        public ActionResult Edit(RoleModel model)
        {
            if (ModelState.IsValid)
            {
                UserProfile user = db.UserProfiles.Find(model.UserId);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // Set new role
                Boolean isAdmin = false;
                if (Roles.IsUserInRole(user.UserName, "Admin"))
                {
                    isAdmin = true;
                }

                if (isAdmin && !model.IsAdmin)
                {
                    Roles.RemoveUserFromRole(user.UserName, "Admin");
                    Roles.AddUserToRole(user.UserName, "Customer");
                }
                else if (!isAdmin && model.IsAdmin)
                {
                    Roles.RemoveUserFromRole(user.UserName, "Customer");
                    Roles.AddUserToRole(user.UserName, "Admin");
                }

                // Set UserProfile
                if (!user.UserName.Equals(model.UserName) || !user.FirstName.Equals(model.FirstName) || !user.LastName.Equals(model.LastName))
                {
                    bool isMyUserNameChanged = false;
                    if ((user.UserId == WebSecurity.CurrentUserId) && !user.UserName.Equals(model.UserName)) isMyUserNameChanged = true;

                    user.UserName = model.UserName;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    if (isMyUserNameChanged)
                    {
                        WebSecurity.Logout();
                    }
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}