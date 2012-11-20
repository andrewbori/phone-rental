using PhoneRental.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace PhoneRental.Controllers
{
    [Authorize(Roles = "Customer")]
    public class MyBorrowsController : Controller
    {
        private PhoneRentalContext db = new PhoneRentalContext();

        //
        // GET: /MyBorrows/

        public ActionResult Index()
        {
            var borrows = db.Borrows.Where(b => b.UserId == WebSecurity.CurrentUserId).
                OrderBy(b => b.Device.DeviceType.Brand.Name).ThenBy(b => b.Device.DeviceType.Type).ThenBy(b => b.Device.AaitIdNumber).ToList();
            if (borrows.Count == 0)
            {
                ViewBag.HasBorrowed = false;
            }
            else
            {
                ViewBag.HasBorrowed = true;
            }

            var preBorrows = db.PreBorrows.Where(pb => pb.UserId == WebSecurity.CurrentUserId).
                OrderBy(b => b.DeviceType.Brand.Name).ThenBy(b => b.DeviceType.Type).ToList();
            if (preBorrows.Count == 0)
            {
                ViewBag.HasPreBorrowed = false;
            }
            else
            {
                ViewBag.HasPreBorrowed = true;
            }

            var model = new MyBorrowModel { Borrows = borrows, PreBorrows = preBorrows };
            return View(model);
        }

    }
}
