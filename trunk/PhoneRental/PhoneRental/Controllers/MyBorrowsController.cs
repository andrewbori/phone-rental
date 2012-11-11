using PhoneRental.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            var userId = db.UserProfiles.Single(
                        p => p.UserName == HttpContext.User.Identity.Name).UserId;

            var borrows = db.Borrows.Where(b => b.UserId == userId).ToList();
            if (borrows.Count == 0)
            {
                ViewBag.HasBorrowed = false;
            }
            else
            {
                ViewBag.HasBorrowed = true;
                ViewBag.Borrows = borrows;
            }
            var preBorrows = db.PreBorrows.Where(pb => pb.UserId == userId).ToList();
            if (preBorrows.Count == 0)
            {
                ViewBag.HasPreBorrowed = false;
            }
            else
            {
                ViewBag.HasPreBorrowed = true;
                ViewBag.PreBorrows = preBorrows;
            }
            return View();
        }

    }
}
