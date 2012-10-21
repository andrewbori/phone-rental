using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhoneRental.Models;

namespace PhoneRental.Controllers
{
    public class BorrowController : Controller
    {
        private PhoneRentalContext db = new PhoneRentalContext();

        //
        // GET: /Borrow/

        public ActionResult Index()
        {
            var preBorrows = db.PreBorrows.Select(d => new
            {
                Id = d.Id,
                Name = d.User.LastName + " " + d.User.FirstName + " (" + d.User.UserName + ") - " +
                    d.DeviceType.Brand.Name + " " + d.DeviceType.Type
            }).OrderBy(d => d.Name);

            ViewBag.PreBorrows = new SelectList(preBorrows, "Id", "Name");

            var users = db.UserProfiles.Select(d => new { UserId = d.UserId, FullName = d.LastName + " " + d.FirstName + " (" + d.UserName + ")" }).OrderBy(d => d.FullName); 
            ViewBag.Users = new SelectList(users, "UserId", "FullName");

            var deviceTypes = db.DeviceTypes.Select(d => new { Id = d.Id, Type = d.Brand.Name + " " + d.Type }).OrderBy(d => d.Type);
            ViewBag.Devices = new SelectList(deviceTypes, "Id", "Type");
            
            return View(db.PreBorrows.ToList());
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
                AAIT = d.AAIT + d.AaitIdNumber.ToString() + " (" + d.Imei + ")" 
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


        //
        // POST: /Borrow/Create

        [HttpPost]
        public ActionResult SaveBorrow(Borrow borrow)
        {
            if (ModelState.IsValid)
            {
                db.Borrows.Add(borrow);
                var device = db.Devices.Single(p => p.Id == borrow.DeviceId);
                var pBorrow = db.PreBorrows.Single(p => p.UserId == borrow.UserId && p.DeviceTypeId == device.DeviceTypeId);
                db.PreBorrows.Remove(pBorrow);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(borrow);
        }

        public ActionResult DeviceList(int DeviceTypeId = 0)
        {
            var devices = db.Devices.Select(p => new { Id = p.Id, DeviceTypeId = p.DeviceTypeId, Name = p.DeviceType.Brand.Name + " " + p.DeviceType.Type}).Where(p => p.DeviceTypeId == DeviceTypeId);

            if (HttpContext.Request.IsAjaxRequest())
            {
                return this.Json(new SelectList(
                    devices,
                    "Id",
                    "Name"), JsonRequestBehavior.AllowGet);
            }
            return View(devices);
        }

        //
        // GET: /Borrow/Details/5

        public ActionResult Details(int id = 0)
        {
            Borrow borrow = db.Borrows.Find(id);
            if (borrow == null)
            {
                return HttpNotFound();
            }
            return View(borrow);
        }

        //
        // GET: /Borrow/Create

        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName");
            ViewBag.DeviceId = new SelectList(db.Devices, "Id", "Imei");
            return View();
        }

        //
        // POST: /Borrow/Create

        [HttpPost]
        public ActionResult Create(Borrow borrow)
        {
            if (ModelState.IsValid)
            {
                db.Borrows.Add(borrow);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName", borrow.UserId);
            ViewBag.DeviceId = new SelectList(db.Devices, "Id", "Imei", borrow.DeviceId);
            return View(borrow);
        }

        //
        // GET: /Borrow/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Borrow borrow = db.Borrows.Find(id);
            if (borrow == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName", borrow.UserId);
            ViewBag.DeviceId = new SelectList(db.Devices, "Id", "Imei", borrow.DeviceId);
            return View(borrow);
        }

        //
        // POST: /Borrow/Edit/5

        [HttpPost]
        public ActionResult Edit(Borrow borrow)
        {
            if (ModelState.IsValid)
            {
                db.Entry(borrow).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName", borrow.UserId);
            ViewBag.DeviceId = new SelectList(db.Devices, "Id", "Imei", borrow.DeviceId);
            return View(borrow);
        }

        //
        // GET: /Borrow/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Borrow borrow = db.Borrows.Find(id);
            if (borrow == null)
            {
                return HttpNotFound();
            }
            return View(borrow);
        }

        //
        // POST: /Borrow/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Borrow borrow = db.Borrows.Find(id);
            db.Borrows.Remove(borrow);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}