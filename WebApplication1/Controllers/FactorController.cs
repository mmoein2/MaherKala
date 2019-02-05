using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class FactorController : Controller
    {
        // GET: Factor
        DBContext db = new DBContext();

        [Authorize(Roles = "Member")]
        public ActionResult Index()
        {
            var email = User.Identity.Name;
            int id = db.Users.Where(p => p.Email == email).FirstOrDefault().Id;
            var order = db.Factors.Where(p => p.User.Id == id).Where(p => p.Status == false).FirstOrDefault();
            List<FactorItem> items = null;
            if (order != null)
                items = db.FactorItems.Include("Product").Where(p => p.Factor.Id == order.Id).ToList();
            ViewBag.Order = order;
            ViewBag.Order_Details = items;
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Member")]
        public ActionResult Shipping()
        {
            var email = User.Identity.Name;
            ViewBag.User = db.Users.Where(p => p.Email == email).FirstOrDefault();

            int id = ViewBag.User.Id;
            var order = db.Factors.Where(p => p.User.Id == id).Where(p => p.Status == false).FirstOrDefault();
            List<FactorItem> items = null;
            if (order != null)
                items = db.FactorItems.Include("Product").Where(p => p.Factor.Id == order.Id).ToList();
            ViewBag.Order = order;
            ViewBag.Order_Details = items;

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Member")]
        public ActionResult Finalize(string Address,string Fullname,string PhoneNumber,string Mobile,string PostalCode)
        {
            if(PostalCode.Length>20|| PostalCode.Length<10)
            {
                TempData["Errors"] = "طول کدپستی باید ده رقم باشد";
                return RedirectToAction("Shipping");
            }
            if (PhoneNumber.Length > 15 || PhoneNumber.Length < 7)
            {
                TempData["Errors"] = "طول ارقام تلفن حداقل ده رقم باشد باشد";
                return RedirectToAction("Shipping");
            }
            if (Mobile.Length > 15 || Mobile.Length < 11)
            {
                TempData["Errors"] = "طول موبایل حداقل باید یازده رقم باشد";
                return RedirectToAction("Shipping");
            }
            if (Address.Length> 1000 || Address.Length < 5)
            {
                TempData["Errors"] = "آدرس را بطور صحیح وارد کنید";
                return RedirectToAction("Shipping");
            }
            if (Address.Length > 50 || Address.Length < 3)
            {
                TempData["Errors"] = "طول نام گیرنده مجاز نیست";
                return RedirectToAction("Shipping");
            }

            var email = User.Identity.Name;
            int id = db.Users.Where(p => p.Email == email).FirstOrDefault().Id;
            var order = db.Factors.Include("FactorItems.Product.Category").Where(p => p.User.Id == id).Where(p => p.Status == false).FirstOrDefault();
            if(order==null)
            {
                throw new Exception();
            }
            if(order.FactorItems==null || order.FactorItems.Count==0)
            {
                throw new Exception();
            }
            foreach (var f in order.FactorItems)
            {
                f.UnitPrice = f.Product.Price - f.Product.Discount;
            }

            order.Date = DateTime.Now;
            order.Address = Address;
            order.Buyer = Fullname;
            order.Mobile = Mobile;
            order.PostalCode = PostalCode;
            order.TotalPrice = order.ComputeTotalPrice()+15000;
            order.Status = true;
            try
            {

            db.SaveChanges();
            }

            catch(DbEntityValidationException ex)
            {
                ViewBag.User = db.Users.Where(p => p.Email == email).FirstOrDefault();

                int iid = ViewBag.User.Id;
                var oorder = db.Factors.Where(p => p.User.Id == iid).Where(p => p.Status == false).FirstOrDefault();
                List<FactorItem> items = null;
                if (oorder != null)
                    items = db.FactorItems.Include("Product").Where(p => p.Factor.Id == oorder.Id).ToList();
                ViewBag.Order = oorder;
                ViewBag.Order_Details = items;
                var errorMessages = ex.EntityValidationErrors
                   .SelectMany(x => x.ValidationErrors)
                   .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("\n ", errorMessages);
                TempData["Errors"] = fullErrorMessage;
                return View("Shipping");

            }
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        [Route("Factor/Store")]
        public ActionResult Store()
        {
            if(!User.Identity.IsAuthenticated)
            {
                return Redirect("/User/Login?ReturnUrl=/Products/Index?Id=" + Request["Id"]);
            }
            int pid = Convert.ToInt32(Request["Id"]);
            var product = db.Products.Include("Category").Where(p => p.Id == pid).Where(p=>p.Status==true).FirstOrDefault();
            
            var tr = db.Database.BeginTransaction();
            if (product.Qty == 0)
            {
                return Redirect("/Products/Index?Id=" + Request["Id"] + "&" + "message=-1");

            }
            var email = User.Identity.Name;
            var id = db.Users.Where(p => p.Email == email).FirstOrDefault().Id;
            var order = db.Factors.Where(p => p.Status == false).Where(p => p.User.Id == id).FirstOrDefault();
            if (order == null)
            {
                order = new Factor();
                order.Status = false;
                order.Date = DateTime.Now;
                order.TotalPrice = 0;
                order.User = db.Users.Find(id);
                order.Buyer = order.User.Fullname;
                order.Address = order.User.Address;
                order.Mobile = order.User.Mobile;
                order.IsAdminShow = false;

                var detail = new FactorItem();
                detail.Product = product;
                detail.ProductName = product.Name;
                detail.Qty = 1;
                detail.UnitPrice = product.Price-product.Discount;

                order.FactorItems.Add(detail);
                db.Factors.Add(order);
                    db.SaveChanges();
            }
            else
            {
                var res = db.FactorItems.Where(p => p.Product.Id == product.Id).Where(p => p.Factor.Id == order.Id).FirstOrDefault();
                if (res != null)
                {
                    if (res.Product.Qty-res.Qty <=0)
                    {
                        return Redirect("/Products/Index?Id=" + Request["Id"] + "&" + "message=-1");

                    }
                    res.Qty++;
                    db.SaveChanges();

                }
                else
                {
                    var detail = new FactorItem();
                    detail.Product = product;
                    detail.ProductName = product.Name;
                    detail.Qty = 1;
                    detail.UnitPrice = product.Price;
                    order.FactorItems.Add(detail);
                    db.SaveChanges();
                }
            }
            tr.Commit();
            return RedirectToAction("Index");

        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var items = db.FactorItems.Include("Product.Category").Where(p => p.Id == id).FirstOrDefault(); ;
            var product = items.Product;
            product.Qty += items.Qty;
            db.FactorItems.Remove(items);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [Authorize]
        public ActionResult ChangeQty()
        {
            var Qty = Convert.ToInt32(Request["Qty"]);

            var fiid = Convert.ToInt32(Request["Id"]);
            var email = User.Identity.Name;

            int id = db.Users.Where(p => p.Email == email).FirstOrDefault().Id;
            var order = db.Factors.Where(p => p.User.Id == id).Where(p => p.Status == false).FirstOrDefault();

            if (order == null)
            {

                ModelState.AddModelError("", "ناموفق");
                List<FactorItem> items = null;
                if (order != null)
                    items = db.FactorItems.Include("Product").Where(p => p.Factor.Id == order.Id).ToList();
                ViewBag.Order = order;
                ViewBag.Order_Details = items;
                return View("Index");
            }

            var data = db.FactorItems.Include("Product.Category").Where(p => p.Factor.Id == order.Id).Where(p => p.Id == fiid).FirstOrDefault();
            if (data == null)
            {
                ModelState.AddModelError("", "ناموفق");
                List<FactorItem> items = null;
                if (order != null)
                    items = db.FactorItems.Include("Product").Where(p => p.Factor.Id == order.Id).ToList();
                ViewBag.Order = order;
                ViewBag.Order_Details = items;
                return View("Index");
            }

            if (data.Product.Qty + data.Qty < Qty)
            {
                ModelState.AddModelError("", "موجودی کافی نیست");
                List<FactorItem> items = null;
                if (order != null)
                    items = db.FactorItems.Include("Product").Where(p => p.Factor.Id == order.Id).ToList();
                ViewBag.Order = order;
                ViewBag.Order_Details = items;
                return View("Index");
            }

            data.Qty = Qty;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles ="Member")]
        public ActionResult History()
        {
            var email = User.Identity.Name;

            int id = db.Users.Where(p => p.Email == email).FirstOrDefault().Id;

            var data = db.Factors.Where(p => p.Status == true).Where(p => p.User.Id == id).OrderByDescending(p => p.Id)
                .ToList();
            ViewBag.Data = data;
            return View();
        }


        [HttpGet]
        [Authorize(Roles = "Member")]
        public ActionResult HistoryItems()
        {
            var email = User.Identity.Name;
            int id = db.Users.Where(p => p.Email == email).FirstOrDefault().Id;
            var fid = Convert.ToInt32(Request["Factor_Id"]);

            var data = db.FactorItems.Include("Product").Where(p => p.Factor.Id == fid).OrderByDescending(p => p.Id).ToList();
            ViewBag.Order_Details = data;
            return View();
        }
    }
}
