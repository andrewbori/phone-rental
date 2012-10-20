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
            ViewBag.PreBorrows = new SelectList(db.PreBorrows.ToList(), "Id", "UserAndDeviceName");
            ViewBag.Users = new SelectList(db.UserProfiles.ToList(), "UserId", "FullUserName");
            ViewBag.Devices = new SelectList(db.DeviceTypes.ToList(), "Id", "DeviceBrandAndType");
            return View(db.PreBorrows.ToList());
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