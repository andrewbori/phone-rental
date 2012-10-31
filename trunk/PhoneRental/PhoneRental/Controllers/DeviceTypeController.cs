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
    public class DeviceTypeController : Controller
    {
        private PhoneRentalContext db = new PhoneRentalContext();

        //
        // GET: /DeviceType/

        public ActionResult Index()
        {
            var devicetypes = db.DeviceTypes.OrderBy(d => d.Brand.Name).Include(d => d.Brand);
            return View(devicetypes.ToList());
        }

        //
        // GET: /DeviceType/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.Brands = db.Brands.OrderBy(b => b.Name).Select(b => b.Name);
            return View();
        }

        //
        // POST: /DeviceType/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(DeviceType devicetype)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var brand = db.Brands.First(b => b.Name.Equals(devicetype.Brand.Name));

                    if (brand != null)
                    {
                        devicetype.Brand = brand;
                    }
                }
                catch (Exception) { }

                db.DeviceTypes.Add(devicetype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Brands = db.Brands.OrderBy(b => b.Name).Select(b => b.Name);
            return View(devicetype);
        }

        //
        // GET: /DeviceType/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id = 0)
        {
            DeviceType devicetype = db.DeviceTypes.Find(id);
            if (devicetype == null)
            {
                return HttpNotFound();
            }
            return View(devicetype);
        }

        //
        // POST: /DeviceType/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(DeviceType devicetype)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var brand = db.Brands.First(b => b.Name.Equals(devicetype.Brand.Name));

                    if (brand != null)
                    {
                        devicetype.Brand = brand;
                        devicetype.BrandId = brand.Id;
                    }
                    else
                    {
                        db.Brands.Add(devicetype.Brand);
                        devicetype.BrandId = devicetype.Brand.Id;
                    }

                }
                catch (Exception)
                {
                    db.Brands.Add(devicetype.Brand);
                    devicetype.BrandId = devicetype.Brand.Id;
                }

                db.Entry(devicetype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(devicetype);
        }

        //
        // GET: /DeviceType/Delete/5
        [Authorize(Roles = "Admin")]
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
        // POST: /DeviceType/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
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