﻿using System;
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
            var profiles = db.UserProfiles.Include("Roles");
            var users = db.UserProfiles.ToList();
            List<string> roles = new List<string>();
            List<string> avatars = new List<string>();
            foreach (var user in users) {
                string role = "Ügyfél";

                if (Roles.IsUserInRole(user.UserName, "Admin"))
                {
                    role = "Adminisztrátor";
                }

                roles.Add(role);
                avatars.Add(AccountController.GetMD5HashData(user.UserName));
            }
            ViewBag.UsersRoles = roles;
            ViewBag.UserAvatars = avatars;

            return View(users);
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
            ViewBag.IsAdmin = isAdmin;

            return View(userprofile);
        }

        //
        // POST: /Role/Edit/5

        [HttpPost]
        public ActionResult Edit(int UserId, Boolean IsAdmin)
        {            
            UserProfile user = db.UserProfiles.Find(UserId);
            if (user == null)
            {
                return HttpNotFound();
            }

            Boolean isAdmin = false;
            if (Roles.IsUserInRole(user.UserName, "Admin"))
            {
                isAdmin = true;
            }

            if (isAdmin && !IsAdmin)
            {
                Roles.RemoveUserFromRole(user.UserName, "Admin");
            }
            else if (!isAdmin && IsAdmin)
            {
                Roles.AddUserToRole(user.UserName, "Admin");
            }

            return RedirectToAction("Index");
        }
    }
}