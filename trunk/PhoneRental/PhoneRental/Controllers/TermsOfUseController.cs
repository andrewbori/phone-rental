using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhoneRental.Controllers
{
    [Authorize(Roles="Customer, Admin")]
    public class TermsOfUseController : Controller
    {
        //
        // GET: /TermsOfUse/

        public ActionResult Index()
        {
            string path = ControllerContext.HttpContext.Server.MapPath("~/Templates/TermsOfUse.cshtml");
            string template = System.IO.File.ReadAllText(path);
            ViewBag.Template = template;
            return View();
        }

    }
}
