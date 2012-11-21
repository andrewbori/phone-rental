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
using PhoneRental.Filters;

namespace PhoneRental.Controllers
{
    [Authorize(Roles = "Admin")]
    [InitializeSimpleMembership]
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

            ViewBag.PreBorrows = new SelectList(preBorrows, "Id", "Name", PreBorrowId);

            ViewBag.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.Deadline = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
            
            var users = db.UserProfiles.Select(d => new { UserId = d.UserId, FullName = d.LastName + " " + d.FirstName + " (" + d.UserName + ")" }).OrderBy(d => d.FullName); 
            ViewBag.Users = new SelectList(users, "UserId", "FullName");

            var deviceTypes = db.DeviceTypes.Select(d => new { Id = d.Id, Type = d.Brand.Name + " " + d.Type }).OrderBy(d => d.Type);

            int deviceTypeId = 0;
            var preBorrow = db.PreBorrows.SingleOrDefault(p => p.Id == PreBorrowId);
            if (preBorrow != null)
            {
                deviceTypeId = preBorrow.DeviceTypeId;
            }
            ViewBag.DeviceTypes = new SelectList(deviceTypes, "Id", "Type", deviceTypeId);

            var devices = deviceListForDeviceType(deviceTypeId);
            ViewBag.Devices = new SelectList(devices, "Id", "Type");

            var borrow = new Borrow();
            borrow.User = new UserProfile();
            return View(borrow);
        }

        [HttpPost]
        public ActionResult NewForPreBorrow(BorrowForPreBorrow model)
        {
            if (HttpContext.Request.IsAjaxRequest())
            {
                if (ModelState.IsValid && !isDeviceBorrowed(model.DeviceId))
                {
                    // Előfoglalás kikeresése
                    PreBorrow preBorrow = db.PreBorrows.Find(model.PreBorrowId);
                    if (preBorrow == null)
                    {
                        return HttpNotFound();
                    }

                    var borrow = new Borrow()
                    {
                        StartDate = model.StartDate,
                        Deadline = model.Deadline,
                        UserId = preBorrow.User.UserId,
                        DeviceId = model.DeviceId,
                        IsBoxOut = model.IsBoxOut,
                        IsChargerOut = model.IsChargerOut,
                        Note = model.Note
                    };

                    db.Borrows.Add(borrow);
                    db.PreBorrows.Remove(preBorrow);
                    db.SaveChanges();
                    return this.Json(new { result = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }
            return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult NewForUser(BorrowForExistingUser model)
        {
            if (HttpContext.Request.IsAjaxRequest())
            {
                if (ModelState.IsValid && !isDeviceBorrowed(model.DeviceId))
                {
                    // Felhasználó kikeresése
                    UserProfile user = db.UserProfiles.Find(model.UserId);
                    if (user == null)
                    {
                        return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
                    }

                    var borrow = new Borrow()
                    {
                        StartDate = model.StartDate,
                        Deadline = model.Deadline,
                        UserId = model.UserId,
                        User = user,
                        DeviceId = model.DeviceId,
                        IsBoxOut = model.IsBoxOut,
                        IsChargerOut = model.IsChargerOut,
                        Note = model.Note
                    };

                    db.Borrows.Add(borrow);
                    db.SaveChanges();
                    return this.Json(new { result = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }

            return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult New(BorrowForNewUser model)
        {
            if (HttpContext.Request.IsAjaxRequest())
            {
                if (ModelState.IsValid && !isDeviceBorrowed(model.DeviceId))
                {
                    string password;
                    try
                    {
                        password = Membership.GeneratePassword(8, 1);
                        WebSecurity.CreateUserAndAccount(model.UserName, password, new { FirstName = model.FirstName, LastName = model.LastName });
                        Roles.AddUserToRole(model.UserName, "Customer");
                    }
                    catch (MembershipCreateUserException e)
                    {
                        Debug.WriteLine(e.Data);
                        return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
                    }

                    try
                    {
                        string path = ControllerContext.HttpContext.Server.MapPath("~/Templates/NewUserEmail.cshtml");
                        string[] to = new String[1];
                        to[0] = model.UserName;
                        NewUserEmail emailModel = new NewUserEmail()
                        {
                            CustomerFirstName = model.FirstName,
                            CustomerLastName = model.LastName,
                            CustomerEmail = model.UserName,
                            CustomerPassword = password
                        };
                        SendEmail.FromTemplate2(path, emailModel, typeof(NewUserEmail), to, "BME-AAIT AMORG telefonkölcsönző regisztráció");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Data);
                        return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
                    }

                    var userid = db.UserProfiles.Where(d => d.UserName == model.UserName).Select(d => d.UserId).Single();

                    var borrow = new Borrow()
                    {
                        StartDate = model.StartDate,
                        Deadline = model.Deadline,
                        DeviceId = model.DeviceId,
                        UserId = userid,
                        IsBoxOut = model.IsBoxOut,
                        IsChargerOut = model.IsChargerOut,
                        Note = model.Note
                    };

                    db.Borrows.Add(borrow);
                    db.SaveChanges();
                    return this.Json(new { result = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }

            return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        public IEnumerable<DeviceSelectModel> deviceListForDeviceType(int DeviceTypeId = 0)
        {
            // Selecting ids of borrowed devices
            var devicesAll = db.Devices.Where(d => d.DeviceTypeId == DeviceTypeId).ToList();
            List<int> ids = new List<int>();
            foreach (var item in devicesAll)
            {
                if (item.Borrow != null)
                {
                    ids.Add(item.Id);
                }
            }

            // Selecting the needed fields
            var devices = db.Devices.Select(d => new
            {
                Id = d.Id,
                DeviceTypeId = d.DeviceTypeId,
                AaitIdNumber = d.AaitIdNumber,
                Imei = d.Imei,
                AAIT = d.DeviceType.AaitIdPattern,
            }).
            Where(d => d.DeviceTypeId == DeviceTypeId).OrderBy(d => d.AaitIdNumber).ToList().
            Select(d => new DeviceSelectModel
            {
                Id = d.Id,
                DeviceTypeId = DeviceTypeId,
                Name = d.AAIT + "_" + d.AaitIdNumber.ToString() + " (" + d.Imei + ")"
            });

            // Filtering the borrowed devices
            foreach (var id in ids)
            {
                devices = devices.Where(d => d.Id != id);
            }

            return devices;
        }

        public ActionResult DeviceListForDeviceType(int DeviceTypeId = 0)
        {
            var devices = deviceListForDeviceType(DeviceTypeId);

            // Creating response
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

        public ActionResult BringBack(int DeviceId)
        {
            String message;
            if (HttpContext.Request.IsAjaxRequest())
            {
                try
                {
                    var borrows = db.Borrows.Where(p => p.DeviceId == DeviceId).Select(p => new { Id = p.Id }).ToList();
                    foreach (var borrow in borrows)
                    {
                        db.Borrows.Remove(db.Borrows.Find(borrow.Id));
                    }
                    db.SaveChanges();
                    message = "OK";
                }
                catch (Exception e)
                {
                    ViewBag.e = e;
                    message = "ERROR";
                }
            }
            else
            {
                message = "ERROR";
            }
            return this.Json(new { result = message }, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        public bool isDeviceBorrowed(int DeviceId)
        {
            var device = db.Devices.SingleOrDefault(d => d.Id == DeviceId);

            return (device.Borrow != null);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}