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
    public class DeviceController : Controller
    {
        private PhoneRentalContext db = new PhoneRentalContext();

        //
        // GET: /Device/

        public ActionResult Index(int type=0)
        {
            var deviceTypes = db.DeviceTypes.Select(d => new { Id = d.Id, Type = d.Brand.Name + " " + d.Type }).OrderBy(d => d.Type);
            if (type == 0) type = deviceTypes.First().Id;
            
            ViewBag.type = new SelectList(deviceTypes, "Id", "Type", type);
            ViewBag.typeId = type;

            var devices = db.Devices.OrderBy(d => d.DeviceType.Brand.Name).ThenBy(d => d.DeviceType.Type).ThenBy(d => d.AaitIdNumber).Include(d => d.DeviceType).Where(d => d.DeviceType.Id == type);
            return View(devices.ToList());
        }

        //
        // GET: /Device/Create

        public ActionResult Create(int type = 0)
        {
            var deviceTypes = db.DeviceTypes.Select(d => new { Id = d.Id, Type = d.Brand.Name + " " + d.Type }).OrderBy(d => d.Type);
            if (type == 0) type = deviceTypes.First().Id;if (type == 0) type = deviceTypes.First().Id;

            var largestIds = db.DeviceTypes.Select(dt => new { Id = dt.Id, LargestId = dt.Devices.Max(d => (int?) d.AaitIdNumber) });

            ViewBag.DeviceTypeId = new SelectList(deviceTypes, "Id", "Type", type);
            ViewBag.AaitIdPattern = new SelectList(db.DeviceTypes, "Id", "AaitIdPattern");
            ViewBag.LargestId = new SelectList(largestIds, "Id", "LargestId");
            
            var devices = new[] { new Device() };
            return View(devices);
        }

        //
        // POST: /Device/Create

        [HttpPost]
        public ActionResult Create(int DeviceTypeId, IEnumerable<Device> devices)
        {
            if (ModelState.IsValid)
            {
                foreach (Device device in devices)
                {
                    device.DeviceTypeId = DeviceTypeId;
                    device.DeviceType = db.DeviceTypes.Find(DeviceTypeId);
                    db.Devices.Add(device);
                    device.DeviceType.Devices.Add(device);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var deviceTypes = db.DeviceTypes.Select(d => new { Id = d.Id, Type = d.Brand.Name + " " + d.Type }).OrderBy(d => d.Type);

            var largestIds = db.DeviceTypes.Select(dt => new { Id = dt.Id, LargestId = dt.Devices.Max(d => (int?)d.AaitIdNumber) });

            ViewBag.DeviceTypeId = new SelectList(deviceTypes, "Id", "Type");
            ViewBag.AaitIdPattern = new SelectList(db.DeviceTypes, "Id", "AaitIdPattern");
            ViewBag.LargestId = new SelectList(largestIds, "Id", "LargestId");

            return View(devices);
        }

        //
        // GET: /Device/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Device device = db.Devices.Find(id);
            if (device == null)
            {
                return HttpNotFound();
            }
            return View(device);
        }

        //
        // POST: /Device/Edit/5

        [HttpPost]
        public ActionResult Edit(Device device)
        {
            if (ModelState.IsValid)
            {
                db.Entry(device).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(device);
        }

        //
        // GET: /Device/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Device device = db.Devices.Find(id);
            if (device == null)
            {
                return HttpNotFound();
            }
            return View(device);
        }

        //
        // POST: /Device/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Device device = db.Devices.Find(id);
            db.Devices.Remove(device);
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