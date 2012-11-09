using PhoneRental.Models;
using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhoneRental.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {

        private PhoneRentalContext db = new PhoneRentalContext();

        //
        // GET: /Statistics/

        public ActionResult Index()
        {
            var sm = new StatisticsModel();
            sm.DevicePopularity = GetDevicePopularity();
            sm.UserActivity = GetUserActivity();
            sm.UserDelay = GetUserDelay();

            return View(sm);
        }

        [NonAction]
        public IEnumerable<ChartModel> GetDevicePopularity()
        {
            /*SELECT TOP 10 d.DeviceTypeId, br.Name, dt.Type, COUNT(d.DeviceTypeId) AS Number FROM Borrow b
            INNER JOIN Device d ON b.DeviceId=d.Id
            INNER JOIN DeviceType dt ON d.DeviceTypeId=dt.Id
            INNER JOIN Brand br ON dt.BrandId=br.Id
            GROUP BY d.DeviceTypeId, br.Name, dt.Type
            ORDER BY Number DESC*/
            var dp = (from b in db.Borrows
                    join d in db.Devices on new { DeviceId = b.DeviceId } equals new { DeviceId = d.Id }
                    join dt in db.DeviceTypes on new { DeviceTypeId = d.DeviceTypeId } equals new { DeviceTypeId = dt.Id }
                    join br in db.Brands on new { BrandId = dt.BrandId } equals new { BrandId = br.Id }
                    group new { d, br, dt } by new
                    {
                        d.DeviceTypeId,
                        br.Name,
                        dt.Type
                    } into g
                    orderby (Int64?)g.Count(p => p.d.DeviceTypeId != null) descending
                    select new ChartModel
                    {
                        Id = g.Key.DeviceTypeId,
                        Name = g.Key.Name + " " + g.Key.Type,
                        Value = g.Count(p => p.d.DeviceTypeId != null)
                    }).Take(10);

            return dp;
        }

        [NonAction]
        public IEnumerable<ChartModel> GetUserActivity()
        {
            /*SELECT TOP 10 u.UserId, u.UserName, COUNT(u.UserName) AS Number
            FROM Borrow b
            INNER JOIN UserProfile u ON b.UserId=u.UserId
            GROUP BY u.UserId, u.UserName
            ORDER BY Number DESC, u.Username*/
            var ua = (from b in db.Borrows
             join u in db.UserProfiles on b.UserId equals u.UserId
             group u by new
             {
                 u.UserId,
                 u.UserName
             } into g
             orderby
               (Int64?)g.Count(p => p.UserName != null) descending,
               g.Key.UserName
             select new ChartModel
             {
                 Id = g.Key.UserId,
                 Name = g.Key.UserName,
                 Value = g.Count(p => p.UserName != null)
             }).Take(10);

            return ua;
        }

        [NonAction]
        public IEnumerable<ChartModel> GetUserDelay()
        {
            /*SELECT TOP 10 u.UserId, u.UserName, MAX(DATEDIFF(DAY, DeadLine, COALESCE(b.EndDate, GetDate()))) AS Number
            FROM UserProfile u
            INNER JOIN Borrow b ON u.UserId=b.UserId
            WHERE (b.EndDate IS NOT NULL AND b.Deadline < b.EndDate) OR (b.EndDate IS NULL AND b.Deadline < GetDate())
            GROUP BY u.UserId, u.UserName
            ORDER BY Number DESC, u.Username*/
            var ud = (from u in db.UserProfiles
                      join b in db.Borrows on u.UserId equals b.UserId
                      where
                        (b.EndDate != null &&
                        b.Deadline < b.EndDate) ||
                        (b.EndDate == null &&
                        b.Deadline < SqlFunctions.GetDate())
                      group new { u, b } by new
                      {
                          u.UserId,
                          u.UserName
                      } into g
                      orderby
                        (System.Int32?)g.Max(p => SqlFunctions.DateDiff("day", p.b.Deadline, ((DateTime?)p.b.EndDate ?? (DateTime?)SqlFunctions.GetDate()))) descending,
                        g.Key.UserName
                      select new ChartModel
                      {
                          Id = g.Key.UserId,
                          Name = g.Key.UserName,
                          Value = (int) g.Max(p => SqlFunctions.DateDiff("day", p.b.Deadline, ((DateTime?)p.b.EndDate ?? (DateTime?)SqlFunctions.GetDate())))
                      }).Take(10);
            
            return ud;
        }

    }
}
