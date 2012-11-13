using System;
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
            try
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

                /* Send email to administrators */
                SendPreBorrowEmail(devicetype, preBorrow.Id);
            }
            catch (Exception e)
            {
                return HttpNotFound(e.Message);
            }
            return RedirectToAction("Index");


            //ViewBag.BrandId = new SelectList(db.Brands, "Id", "Name", devicetype.BrandId);
            //return View(devicetype);
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
            try
            {
                var userId = db.UserProfiles.Single(
                        p => p.UserName == HttpContext.User.Identity.Name).UserId;

                var preBorrowId = db.PreBorrows.Where(pb => pb.DeviceTypeId == id && pb.UserId == userId).Select(pb => pb.Id).Single();
                var preBorrow = db.PreBorrows.Find(preBorrowId);
                db.PreBorrows.Remove(preBorrow);
                db.SaveChanges();

                return RedirectToAction("","PreBorrow");
            }
            catch (Exception e)
            {
                return HttpNotFound(e.Message);
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
            string link = Request.Url.Host + ":" + Request.Url.Port + @"/Borrow?PreBorrowId=" + idPreBorrow;
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