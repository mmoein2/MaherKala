using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers.Marketer
{
    public class MarketerUserController : Controller
    {
        // GET: Admin/MarketerUser
        DBContext db = new DBContext();

        // GET: Admin/User
        public ActionResult Index()
        {
            var url = "/Admin/MarketerUser/Index?";
            string name = Request["LastName"];
            string mobile = Request["Mobile"];
            var query = db.MarketerUsers.AsQueryable();
            if (Request["LastName"] != null && Request["LastName"] != "")
            {
                query = query.Where(p => p.LastName.Contains(name));
                if (url.Contains("?"))
                    url = url + "&LastName=" + name;
                else
                    url = url + "?LastName=" + name;
            }
            if (Request["Mobile"] != null && Request["Mobile"] != "")
            {
                query = query.Where(p => p.Mobile.Contains(mobile));
                if (url.Contains("?"))
                    url = url + "&Mobile=" + mobile;
                else
                    url = url + "?Mobile=" + mobile;
            }
            var ordered = query.OrderByDescending(p => p.Id);
            var paginated = new PagedItem<MarketerUser>(ordered, url);
            ViewBag.Data = paginated;
            return View();
        }
        [HttpGet]
        public ActionResult Deactive(int id)
        {
            var user = db.MarketerUsers.Find(id);
            user.IsAvailable = false;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Active(int id)
        {
            var user = db.MarketerUsers.Find(id);
            user.IsAvailable = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Details(int id)
        {
            var user = db.MarketerUsers.Find(id);
            ViewBag.Data = user;
            return View();
        }
    }
}