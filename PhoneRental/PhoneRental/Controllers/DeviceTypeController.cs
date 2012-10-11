﻿using System;
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
            return View(db.DeviceTypes.ToList());
        }

        //
        // GET: /DeviceType/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /DeviceType/Create

        [HttpPost]
        public ActionResult Create(DeviceType devicetype)
        {
            if (ModelState.IsValid)
            {
                db.DeviceTypes.Add(devicetype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

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
            if (ModelState.IsValid)
            {
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