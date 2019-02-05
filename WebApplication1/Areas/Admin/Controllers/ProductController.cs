using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        DBContext db = new DBContext();
        public ActionResult ZeroStock()
        {
            var data = db.Products.Include("Category").Where(p => p.Qty == 0).OrderByDescending(p=>p.Id);
            ViewBag.Data = new PagedItem<Product>(data, "/Admin/Product/ZeroStock");
            return View();
        }
        // GET: Admin/Product
        public ActionResult Index()
        {
            var url = "/Admin/Product/Index";
            var data = db.Products.Include("Category").AsQueryable();

            int? category_id = Convert.ToInt32(Request["Category_id"]);
            var name = Request["Name"];
            if (category_id!=null&category_id!=0)
            {
                data = data.Where(p => p.Category.Id == category_id);
                if(url.Contains("?"))
                    url = url + "&Category_Id=" + category_id;
                else
                    url=url+ "?Category_Id=" + category_id;
            }
            if (name != null&name!="")
            {
                data = data.Where(p => p.Name.Contains(name));
                if (url.Contains("?"))
                    url = url + "&Name=" + name;
                else
                    url = url + "?Name=" + name;
            }

            var res=data.OrderByDescending(p => p.Id);
            
            ViewBag.Data = new PagedItem<Product>(res, url);
            var categories = db.Database.SqlQuery<Category>("select * from Categories c where Id Not In (select Parent_Id from Categories where Parent_Id Is Not Null) and  c.Parent_Id Is Not null order by Parent_Id");
            ViewBag.Categories = categories;
            
            return View();
        }
        public ActionResult Create()
        {

            var data = db.Database.SqlQuery<Category>("select * from Categories c where Id Not In (select Parent_Id from Categories where Parent_Id Is Not Null) and  c.Parent_Id Is Not null order by Parent_Id");
            ViewBag.Categories = data;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Store(Product product)
        {
            if(Convert.ToInt32(Request["Category_Id"])==-1)
            {
                TempData["Error"] = "دسته بندی را انتخاب کنید";
                return RedirectToAction("Create");
            }
            if ((Request["Name"]) == "")
            {
                TempData["Error"] = "نام را انتخاب کنید";
                return RedirectToAction("Create");
            }
            if ((Request["Desc"]) == "")
            {
                TempData["Error"] = "توضیحات را انتخاب کنید";
                return RedirectToAction("Create");
            }
            if (Convert.ToInt32(Request["Price"]) <=0)
            {
                TempData["Error"] = "مبلغ را انتخاب کنید";
                return RedirectToAction("Create");
            }

            product.Status = true;
            var category_id = Request["Category_Id"];
            Category c = db.Categories.Find(Convert.ToInt32(category_id));
            product.Category = c;
            var img = Request.Files["Main_Image"];
            if(img==null || img.FileName=="")
            {
                TempData["Error"] = "تصویر را انتخاب کنید";
                return RedirectToAction("Create");

            }
            if (!(img.ContentType == "image/jpeg" || img.ContentType == "image/png" || img.ContentType == "image/bmp"))
            {
                TempData["Error"] = "نوع تصویر غیر قابل قبول است";
                return RedirectToAction("Create");
            }

            var name = Guid.NewGuid().ToString().Replace('-', '0') + "." + img.FileName.Split('.')[1];

            var imageUrl = "/Upload/Products/" + name;
            string path = Server.MapPath(imageUrl);
            img.SaveAs(path);
            product.Main_Image = imageUrl;

            var image = Image.FromStream(img.InputStream, true, true);
            var thumb = ImageResizer.RezizeImage(image, 250, 250);
            name = Guid.NewGuid().ToString().Replace('-', '0') + "." + img.FileName.Split('.')[1];

            imageUrl = "/Upload/Products/" + name;
            path = Server.MapPath(imageUrl);
            thumb.Save(path);
            product.Thumbnail = imageUrl;

            HttpFileCollectionBase hfc = Request.Files;
            for (int i = 0; i < hfc.Count; i++)
            {
                HttpPostedFileBase hpf = hfc[i];
                if (hfc[i].FileName == Request.Files["Main_Image"].FileName) continue;
                if (hfc[i].FileName == "") continue;
                if (!(hpf.ContentType == "image/jpeg" || hpf.ContentType == "image/png" || hpf.ContentType == "image/bmp"))
                {
                    TempData["Error"] = "نوع تصویر غیر قابل قبول است";
                    return RedirectToAction("Create");
                }


                if (hpf.ContentLength > 0)
                {
                    img = hfc[i];
                    name = Guid.NewGuid().ToString().Replace('-', '0') + "." + img.FileName.Split('.')[1];

                    imageUrl = "/Upload/Products/" + name;
                    path = Server.MapPath(imageUrl);
                    img.SaveAs(path);
                    if (product.Images != null)
                        product.Images += ";";
                    product.Images += imageUrl;

                }
            }
           

                db.Products.Add(product);
                db.SaveChanges();

            
        
            return Redirect("/Admin/Product/Index");
        }
        [HttpGet]
        public ActionResult Deactivate(int id)
        {

            var product = db.Products.Include("Category").Where(p => p.Id == id).FirstOrDefault();
            product.Status = false;
            db.SaveChanges();


            return Redirect(Request.UrlReferrer.ToString());
        }
        [HttpGet]
        public ActionResult Active(int id)
        {

            var product = db.Products.Include("Category").Where(p => p.Id == id).FirstOrDefault();
            product.Status = true;
            db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var data = db.Database.SqlQuery<Category>("select * from Categories c where Id Not In (select Parent_Id from Categories where Parent_Id Is Not Null) and  c.Parent_Id Is Not null");
            ViewBag.Categories = data;
            var product = db.Products.Include("Category").Where(p => p.Id == id).FirstOrDefault();
            ViewBag.Data = product;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Product/Update")]
        public ActionResult Update(Product product)
        {
            if (Convert.ToInt32(Request["Category_Id"]) == -1)
            {
                TempData["Error"] = "دسته بندی را انتخاب کنید";
                return Redirect("/Admin/Product/Edit/"+product.Id);
            }
            if ((Request["Name"]) == "")
            {
                TempData["Error"] = "نام را انتخاب کنید";
                return Redirect("/Admin/Product/Edit/" + product.Id);
            }
            if ((Request["Desc"]) == "")
            {
                TempData["Error"] = "توضیحات را انتخاب کنید";
                return Redirect("/Admin/Product/Edit/" + product.Id);
            }
            if (Convert.ToInt32(Request["Price"]) <= 0)
            {
                TempData["Error"] = "مبلغ را انتخاب کنید";
                return Redirect("/Admin/Product/Edit/"+product.Id);
            }
            var update = db.Products.Include("Category").Where(p=>p.Id==product.Id).FirstOrDefault();
            var cid = Convert.ToInt32(Request["Category_Id"]);
            var category = db.Categories.Where(p => p.Id == cid).FirstOrDefault();
            update.Name = product.Name;
            update.Desc = product.Desc;
            update.Discount = product.Discount;
            update.Price = product.Price;
            update.Qty = product.Qty;
            update.Tags = product.Tags;
            update.Category= category;

            var img = Request.Files["Main_Image"];
            if (img.ContentLength>0)
            {

                System.IO.File.Delete(Server.MapPath(update.Main_Image));
                System.IO.File.Delete(Server.MapPath(update.Thumbnail));

                if (!(img.ContentType == "image/jpeg" || img.ContentType == "image/png" || img.ContentType == "image/bmp"))
                    throw new DbEntityValidationException("نوع فایل غیر قابل قبول است");
                var name = Guid.NewGuid().ToString().Replace('-', '0') + "." + img.FileName.Split('.')[1];

                var imageUrl = "/Upload/Products/" + name;
                string path = Server.MapPath(imageUrl);
                img.SaveAs(path);
                update.Main_Image = imageUrl;

                var image = Image.FromStream(img.InputStream, true, true);
                var thumb = ImageResizer.RezizeImage(image, 250, 250);
                name = Guid.NewGuid().ToString().Replace('-', '0') + "." + img.FileName.Split('.')[1];

                imageUrl = "/Upload/Products/" + name;
                path = Server.MapPath(imageUrl);
                thumb.Save(path);
                update.Thumbnail = imageUrl;
            }

            HttpFileCollectionBase hfc = Request.Files;
            if (hfc[1].ContentLength > 0)
            {
                if (((Request.Files["Main_Image"].ContentLength>0 && hfc[1].ContentLength> 0) || (Request.Files["Main_Image"].ContentLength == 0 && hfc[1].ContentLength > 0)))
                {

                    var array = update.Images.Split(';');
                    for (int i = 0; i < array.Length; i++)
                    {
                        System.IO.File.Delete(Server.MapPath(array[i]));
                    }

                    update.Images = "";

                    for (int i = 0; i < hfc.Count; i++)
                    {
                        HttpPostedFileBase hpf = hfc[i];
                        if (hpf.ContentLength == 0) continue;
                        if (!(hpf.ContentType == "image/jpeg" || hpf.ContentType == "image/png" || hpf.ContentType == "image/bmp"))
                            throw new DbEntityValidationException("نوع فایل غیر قابل قبول است");
                        if (Request.Files["Main_Image"].ContentLength>0)
                            if (hfc[i].FileName == Request.Files["Main_Image"].FileName) continue;

                       
                            img = hfc[i];
                            var name = Guid.NewGuid().ToString().Replace('-', '0') + "." + img.FileName.Split('.')[1];

                            var imageUrl = "/Upload/Products/" + name;
                            var path = Server.MapPath(imageUrl);
                            img.SaveAs(path);
                            if (update.Images != "")
                                update.Images += ";";
                            update.Images += imageUrl;

                        
                    }
                }


            }

            try
            {

                db.SaveChanges();

            }
            catch (DbEntityValidationException ex)
            {

            }
            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}