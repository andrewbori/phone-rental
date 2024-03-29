﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhoneRental.Models;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using RazorEngine;
using System.Web.Security;

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


        public ActionResult New(int DeviceTypeId)
        {
            DeviceType devicetype;
            if (HttpContext.Request.IsAjaxRequest())
            {
                try
                {
                    devicetype = db.DeviceTypes.Find(DeviceTypeId);
                    if (devicetype == null)
                    {
                        return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
                    }
                    var preBorrow = new PreBorrow()
                    {
                        Date = DateTime.Now,
                        DeviceTypeId = devicetype.Id,
                        UserId = db.UserProfiles.Single(
                            p => p.UserName == HttpContext.User.Identity.Name).UserId
                    };

                    var exist = (from pb in db.PreBorrows
                                 where ((pb.DeviceTypeId == preBorrow.DeviceTypeId) &&
                                 (pb.UserId == preBorrow.UserId))
                                 select pb.Id).Count();
                    if (exist == 0)
                    {
                        db.PreBorrows.Add(preBorrow);
                        db.SaveChanges();
                    }
                    else
                    {
                        return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
                    }

                    /* Send email to administrators */
                    SendPreBorrowEmail(devicetype, preBorrow.Id);
                    return this.Json(new { result = "OK" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ViewBag.e = e;
                    return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // GET: /PreBorrow/Delete/5

        public ActionResult Delete(int DeviceTypeId)
        {
            DeviceType devicetype = db.DeviceTypes.Find(DeviceTypeId);
            if (HttpContext.Request.IsAjaxRequest())
            {
                if (devicetype == null)
                {
                    return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
                }
                try
                {
                    var userId = db.UserProfiles.Single(
                            p => p.UserName == HttpContext.User.Identity.Name).UserId;

                    var preBorrowId = db.PreBorrows.Where(pb => pb.DeviceTypeId == DeviceTypeId && pb.UserId == userId).Select(pb => pb.Id).Single();
                    var preBorrow = db.PreBorrows.Find(preBorrowId);
                    db.PreBorrows.Remove(preBorrow);
                    db.SaveChanges();
                    return this.Json(new { result = "OK" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return this.Json(new { result = "ERROR" }, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]
        private void SendPreBorrowEmail(DeviceType devicetype, int idPreBorrow)
        {
            var brandname = (from dt in db.DeviceTypes
                             where dt.Id == devicetype.Id
                             select dt.Brand.Name).First();
            var type = (from dt in db.DeviceTypes
                        where dt.Id == devicetype.Id
                        select dt.Type).First();
            var firstname = db.UserProfiles.Single(
                    p => p.UserName == HttpContext.User.Identity.Name).FirstName;
            var lastname = db.UserProfiles.Single(
                    p => p.UserName == HttpContext.User.Identity.Name).LastName;
            string link = "http://"+Request.Url.Host + ":" + Request.Url.Port + @"/Borrow?PreBorrowId=" + idPreBorrow;
            var model = new PreBorrowEmail()
            {
                CustomerFirstName = firstname,
                CustomerLastName = lastname,
                CustomerEmail = HttpContext.User.Identity.Name,
                Available = devicetype.Availability,
                Date = DateTime.Now.ToString(),
                DeviceType = brandname + " " + type,
                Link = link

            };
            string path = ControllerContext.HttpContext.Server.MapPath("~/Templates/PreBorrowEmail.cshtml");
            string[] to = Roles.GetUsersInRole("Admin");

            SendEmail.FromTemplate(path, model, typeof(PreBorrowEmail), to, "Új előfoglalás");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}