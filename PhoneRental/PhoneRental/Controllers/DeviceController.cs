﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhoneRental.Models;
using System.IO;

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
            if (type == 0 && deviceTypes.Count() > 0) type = deviceTypes.First().Id;
            
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
            if (type == 0 && deviceTypes.Count() > 0) type = deviceTypes.First().Id;

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
                bool isValid = true;
                Device[] deviceArray = devices.ToArray();

                // Checking whether the devices are different or not.
                for (int i = 0; i < deviceArray.Length && isValid; i++)
                {
                    for (int j = i + 1; j < deviceArray.Length; j++)
                    {
                        if (deviceArray[i].AaitIdNumber == deviceArray[j].AaitIdNumber ||
                            deviceArray[i].Imei == deviceArray[j].Imei)
                        {
                            isValid = false;
                            break;
                        }
                    }
                }

                if (isValid)
                {
                    // Checking whether the given devices are unique or not.
                    foreach (Device device in devices)
                    {
                        if (!isAaitIdNumberUnique(DeviceTypeId, device.AaitIdNumber) || !isImeiUnique(device.Imei))
                        {
                            isValid = false;
                            break;
                        }
                    }
                }

                if (isValid)
                {
                    // Adding the given devices.
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
            if (ModelState.IsValid && 
                isAaitIdNumberUnique(device.DeviceTypeId, device.AaitIdNumber, device.Id) && 
                isImeiUnique(device.Imei, device.Id))
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

        [HttpPost]
        public JsonResult IsAaitIdNumberUnique(int? Id=0)
        {
            int DeviceTypeId = 0;
            int AaitIdNumber = 0;
            if (Id == 0)
            {
                using (var reader = new StreamReader(Request.InputStream))
                {
                    string content = reader.ReadToEnd();

                    string[] parts = content.Split('&');
                    int.TryParse(getValue(parts, "DeviceTypeId"), out DeviceTypeId);
                    int.TryParse(getValue(parts, "AaitIdNumber"), out AaitIdNumber);
                }
            }
            else
            {
                int.TryParse(Request.Form.Get("DeviceTypeId"), out DeviceTypeId);
                int.TryParse(Request.Form.Get("AaitIdNumber"), out AaitIdNumber);
            }

            bool result = isAaitIdNumberUnique(DeviceTypeId, AaitIdNumber, Id);

            return Json(result);
        }

        [HttpPost]
        public JsonResult IsImeiUnique(int? Id=0)
        {
            string Imei = null;
            if (Id == 0)
            {
                using (var reader = new StreamReader(Request.InputStream))
                {
                    string content = reader.ReadToEnd();

                    string[] parts = content.Split('&');
                    Imei = getValue(parts, "Imei");
                }
            }
            else
            {
                Imei = Request.Form.Get("Imei");
            }

            bool result = isImeiUnique(Imei, Id);

            return Json(result);
        }

        [NonAction]
        public bool isAaitIdNumberUnique(int DeviceTypeId, int AaitIdNumber, int? Id = 0)
        {
            if (DeviceTypeId == 0 || AaitIdNumber == 0)
            {
                return true;
            }

            bool result = !db.Devices.Where(d => d.Id != Id).Any(d => d.DeviceTypeId == DeviceTypeId && d.AaitIdNumber == AaitIdNumber);

            return result;
        }

        [NonAction]
        public bool isImeiUnique(string Imei, int? Id = 0)
        {
            if (Imei == null)
            {
                return true;
            }

            bool result = !db.Devices.Where(d => d.Id != Id).Any(d => d.Imei == Imei);

            return result;
        }

        [NonAction]
        public string getValue(string[] parts, string key)
        {
            foreach (string part in parts)
            {
                string[] pair = part.Split('=');
                if (pair.Length == 2)
                {
                    if (pair[0].IndexOf(key) != -1)
                    {
                        return pair[1];
                    }
                }
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}