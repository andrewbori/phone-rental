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
    [Authorize(Roles = "Admin")]
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
        public ActionResult Create()
        {
            ViewBag.Brands = db.Brands.OrderBy(b => b.Name).Select(b => b.Name);
            return View();
        }

        //
        // POST: /DeviceType/Create
        [HttpPost]
        public ActionResult Create(DeviceType devicetype)
        {
            if (ModelState.IsValid && 
                isAaitIdPatternUnique(devicetype.AaitIdPattern) && 
                isTypeUnique(devicetype.Type, devicetype.Brand))
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
        public ActionResult Edit(DeviceType devicetype)
        {
            if (ModelState.IsValid &&
                isAaitIdPatternUnique(devicetype.AaitIdPattern, devicetype.Id) &&
                isTypeUnique(devicetype.Type, devicetype.Brand, devicetype.Id))
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

        [HttpPost]
        public JsonResult IsTypeUnique(string type, Brand Brand, int? Id=0)
        {
            bool result = isTypeUnique(type, Brand, Id);

            return Json(result);
        }

        [HttpPost]
        public JsonResult IsAaitIdPatternUnique(string AaitIdPattern, int? Id=0)
        {
            bool result = isAaitIdPatternUnique(AaitIdPattern, Id);

            return Json(result);
        }

        [HttpPost]
        private bool isTypeUnique(string type, Brand Brand, int? Id = 0)
        {
            if (type == null || Brand == null)
            {
                return true;
            }

            bool result = !db.DeviceTypes.Where(d => d.Id != Id).Any(d => d.Type == type && d.Brand.Name == Brand.Name);

            return result;
        }

        [NonAction]
        private bool isAaitIdPatternUnique(string AaitIdPattern, int? Id = 0)
        {
            if (AaitIdPattern == null)
            {
                return true;
            }

            bool result = !db.DeviceTypes.Where(d => d.Id != Id).Any(d => d.AaitIdPattern == AaitIdPattern);

            return result;
        }

        [NonAction]
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}