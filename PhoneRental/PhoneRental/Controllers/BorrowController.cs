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
using System.Diagnostics;
using System.Configuration;

namespace PhoneRental.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BorrowController : Controller
    {
        private PhoneRentalContext db = new PhoneRentalContext();

        //
        // GET: /Borrow/

        [HttpGet]
        public ActionResult Index(int PreBorrowId = 0)
        {
            var preBorrows = db.PreBorrows.Select(d => new
            {
                Id = d.Id,
                Name = d.User.LastName + " " + d.User.FirstName + " (" + d.User.UserName + ") - " +
                    d.DeviceType.Brand.Name + " " + d.DeviceType.Type
            }).OrderBy(d => d.Name);

            ViewBag.PreBorrows = new SelectList(preBorrows, "Id", "Name",PreBorrowId);
            ViewBag.StartDate = DateTime.Now.ToString("MM/dd/yyyy");
            ViewBag.Deadline = DateTime.Now.AddDays(30).ToString("MM/dd/yyyy");
            

            var users = db.UserProfiles.Select(d => new { UserId = d.UserId, FullName = d.LastName + " " + d.FirstName + " (" + d.UserName + ")" }).OrderBy(d => d.FullName); 
            ViewBag.Users = new SelectList(users, "UserId", "FullName");

            var deviceTypes = db.DeviceTypes.Select(d => new { Id = d.Id, Type = d.Brand.Name + " " + d.Type }).OrderBy(d => d.Type);
            ViewBag.DeviceTypes = new SelectList(deviceTypes, "Id", "Type");

            var devices = db.DeviceTypes.Where(d => d.Id == 0).Select(d => new { Id = d.Id, Type = d.Brand.Name + " " + d.Type }).OrderBy(d => d.Type);
            
            ViewBag.Devices = new SelectList(devices, "Id", "Type");

            var borrow = new Borrow();
            borrow.User = new UserProfile();
            return View(borrow);
        }


        public ActionResult WithPreBorrow(int id = 0)
        {
            PreBorrow preBorrow = db.PreBorrows.Find(id);
            if (preBorrow == null)
            {
                return HttpNotFound();
            }

            var devices = db.Devices.Select(d => new
            {
                Id = d.Id,
                DeviceTypeId = d.DeviceTypeId,
                AaitIdNumber = d.AaitIdNumber,
                Imei = d.Imei,
                AAIT = d.DeviceType.AaitIdPattern
            }).Where(d => d.DeviceTypeId == preBorrow.DeviceTypeId).OrderBy(d => d.AAIT).ToList().Select( d => new
            {
                Id = d.Id,
                AAIT = d.AAIT + "_" + d.AaitIdNumber.ToString() + " (" + d.Imei + ")" 
            });
            ViewBag.Devices = new SelectList(devices, "Id", "AAIT");
            ViewBag.PreBorrow = preBorrow;

            var borrow = new Borrow()
            {
                UserId = preBorrow.UserId,
                StartDate = DateTime.Now
            };

            return View(borrow);
        }

        [HttpPost]
        public ActionResult NewForPreBorrow(DateTime Deadline, DateTime StartDate, int DeviceId, int PreBorrowId)
        {
            // Előfoglalás kikeresése
            PreBorrow preBorrow = db.PreBorrows.Find(PreBorrowId);
            if (preBorrow == null)
            {
                return HttpNotFound();
            }
            var borrow = new Borrow()
            {
                StartDate = StartDate,
                Deadline = Deadline,
                UserId = preBorrow.User.UserId,
                DeviceId = DeviceId
            };
            if (ModelState.IsValid)
            {
                db.Borrows.Add(borrow);
                db.PreBorrows.Remove(preBorrow);
                db.SaveChanges();
                return View();
            }
            
            
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult NewForUser(int UserId, int DeviceId, DateTime Deadline)
        {
            // Előfoglalás kikeresése
            UserProfile user = db.UserProfiles.Find(UserId);
            if (user == null)
            {
                return HttpNotFound();
            }

            var borrow = new Borrow()
            {
                StartDate = DateTime.Now,
                Deadline = Deadline,
                UserId = user.UserId,
                User = user,
                DeviceId = DeviceId
            };
            if (ModelState.IsValid)
            {
                db.Borrows.Add(borrow);
                db.SaveChanges();
                return View();
            }


            return HttpNotFound();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult New(UserProfile user, int DeviceId, DateTime Deadline)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(user.UserName, user.UserName, new { FirstName = user.FirstName, LastName = user.LastName });
                    Roles.AddUserToRole(user.UserName, "Customer");
                }
                catch (MembershipCreateUserException e)
                {
                    Debug.WriteLine(e.Data);
                }
            }

            var userid = db.UserProfiles.Where(d => d.UserName == user.UserName).Select(d => d.UserId).Single();

            var borrow = new Borrow()
            {
                StartDate = DateTime.Now,
                Deadline = Deadline,
                DeviceId = DeviceId,
                UserId = userid
            };
            if (ModelState.IsValid)
            {
                db.Borrows.Add(borrow);
                db.SaveChanges();
                return View();
            }


            return HttpNotFound();
        }

        public ActionResult DeviceListForDeviceType(int DeviceTypeId = 0)
        {
            var devices = db.Devices.Select(p => new { Id = p.Id, DeviceTypeId = p.DeviceTypeId, Name = p.DeviceType.AaitIdPattern + "_" + p.Imei}).Where(p => p.DeviceTypeId == DeviceTypeId).ToList();

            devices.Insert(0, new { Id = 0, DeviceTypeId = 0, Name = "Kérem válasszon!" });

            if (HttpContext.Request.IsAjaxRequest())
            {
                return this.Json(new SelectList(
                    devices,
                    "Id",
                    "Name"), JsonRequestBehavior.AllowGet);
            }
            return View(devices);
        }

        public ActionResult DeviceListForPreBorrow(int PreBorrowId = 0)
        {
            PreBorrow preBorrow = db.PreBorrows.Find(PreBorrowId);
            int deviceTypeId;
            if (preBorrow == null)
            {
                deviceTypeId = 0;
            }
            else
            {
                deviceTypeId = preBorrow.DeviceTypeId;
            }
            return DeviceListForDeviceType(deviceTypeId);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}