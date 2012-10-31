using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhoneRental.Models;
using System.Web.UI;

namespace PhoneRental.Controllers
{
    [Authorize(Roles = "Customer")]
    public class PreBorrowController : Controller
    {
        private PhoneRentalContext db = new PhoneRentalContext();

        //
        // GET: /PreBorrow/

        public ActionResult Index()
        {
            var devicetypes = db.DeviceTypes.Include(d => d.Brand);
            return View(devicetypes.ToList());
        }

        //
        // GET: /PreBorrow/New/5

        public ActionResult New(int id = 0)
        {
            DeviceType devicetype = db.DeviceTypes.Find(id);
            if (devicetype == null)
            {
                return HttpNotFound();
            }
            ViewBag.BrandId = new SelectList(db.Brands, "Id", "Name", devicetype.BrandId);
            return View(devicetype);
        }

        //
        // POST: /PreBorrow/Edit/5

        [HttpPost]
        public ActionResult New(DeviceType devicetype)
        {
            var preBorrow = new PreBorrow()
            {
                Date = DateTime.Now,
                DeviceTypeId = devicetype.Id,
                UserId = db.UserProfiles.Single(
                    p => p.UserName == HttpContext.User.Identity.Name).UserId
            };
            db.PreBorrows.Add(preBorrow);
            db.SaveChanges();
            return RedirectToAction("Index");


            //ViewBag.BrandId = new SelectList(db.Brands, "Id", "Name", devicetype.BrandId);
            //return View(devicetype);
        }

        //
        // GET: /PreBorrow/Details/5

        public ActionResult Details(int id = 0)
        {
            DeviceType devicetype = db.DeviceTypes.Find(id);
            if (devicetype == null)
            {
                return HttpNotFound();
            }
            return View(devicetype);
        }

        //
        // GET: /PreBorrow/Create

        public ActionResult Create()
        {
            ViewBag.BrandId = new SelectList(db.Brands, "Id", "Name");
            return View();
        }

        //
        // POST: /PreBorrow/Create

        [HttpPost]
        public ActionResult Create(DeviceType devicetype)
        {
            if (ModelState.IsValid)
            {
                db.DeviceTypes.Add(devicetype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BrandId = new SelectList(db.Brands, "Id", "Name", devicetype.BrandId);
            return View(devicetype);
        }

        //
        // GET: /PreBorrow/Edit/5

        public ActionResult Edit(int id = 0)
        {
            DeviceType devicetype = db.DeviceTypes.Find(id);
            if (devicetype == null)
            {
                return HttpNotFound();
            }
            ViewBag.BrandId = new SelectList(db.Brands, "Id", "Name", devicetype.BrandId);
            return View(devicetype);
        }

        //
        // POST: /PreBorrow/Edit/5

        [HttpPost]
        public ActionResult Edit(DeviceType devicetype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(devicetype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BrandId = new SelectList(db.Brands, "Id", "Name", devicetype.BrandId);
            return View(devicetype);
        }

        //
        // GET: /PreBorrow/Delete/5

        public ActionResult Delete(int id = 0)
        {
            DeviceType devicetype = db.DeviceTypes.Find(id);
            if (devicetype == null)
            {
                return HttpNotFound();
            }
            return View(devicetype);
        }

        //
        // POST: /PreBorrow/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DeviceType devicetype = db.DeviceTypes.Find(id);
            db.DeviceTypes.Remove(devicetype);
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