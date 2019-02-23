using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebApplication1.Filter;
using WebApplication1.Models;

namespace WebApplication1.Controllers.api.Marketer
{
    public class MarketerFactorController : ApiController
    {
        DBContext db = new DBContext();

        [MarketerAuthorize]
        [HttpPost]
        [Route("api/MarketerFactor/Store")]
        public object Store()
        {
            try
            {
                var tr = db.Database.BeginTransaction();
                int pid = Convert.ToInt32(HttpContext.Current.Request.Form["Product_Id"]);
                var product = db.Products.Include("Category").Where(p => p.Id == pid).FirstOrDefault();

                if (product.Qty == 0)
                {
                    return new { Message = 1 };

                }

                var token = HttpContext.Current.Request.Form["Api_Token"];
                var factor_id = Convert.ToInt32(HttpContext.Current.Request.Form["Factor_Id"]);

                var marketer = db.MarketerUsers.Where(p => p.Api_Token == token).FirstOrDefault();

                if (factor_id == 0)
                {
                    if (marketer.FactorCounter >= 5)
                    {
                        return new { Message = 4 };
                    }
                    var order = new MarketerFactor();
                    var BuyerAddress = (HttpContext.Current.Request.Form["BuyerAddress"]);
                    var Buyer = (HttpContext.Current.Request.Form["Buyer"]);
                    var BuyerMobile = (HttpContext.Current.Request.Form["BuyerMobile"]);
                    var BuyerPhoneNumber = HttpContext.Current.Request.Form["BuyerPhoneNumber"];
                    var BuyerPostalCode = (HttpContext.Current.Request.Form["BuyerPostalCode"]);
                    order.Status = 1;
                    order.Date = DateTime.Now;

                    order.MarketerUser = marketer;
                    order.Buyer = Buyer;
                    order.BuyerAddress = BuyerAddress;
                    order.BuyerMobile = BuyerMobile;
                    order.BuyerPhoneNumber = BuyerPhoneNumber;
                    order.BuyerPostalCode = BuyerPostalCode;
                    order.IsAdminCheck = false;
                    order.IsAdminShow = false;

                    var detail = new MarketerFactorItem();
                    detail.Product = product;
                    detail.ProductName = product.Name;
                    detail.Qty = 1;
                    detail.UnitPrice = product.Price - product.Discount;

                    order.MarketerFactorItems.Add(detail);
                    db.MarketerFactor.Add(order);
                    marketer.FactorCounter++;
                    db.SaveChanges();



                }
                else
                {
                    var order = db.MarketerFactor.FirstOrDefault(p => p.Id == factor_id && p.Status == 1);
                    if (order == null)
                    {
                        return new { Message = 3 };
                    }
                    var res = db.MarketerFactorItem.Where(p => p.Product.Id == product.Id).Where(p => p.MarketerFactor.Id == order.Id).FirstOrDefault();
                    if (res != null)
                    {
                        if (res.Product.Qty - res.Qty <= 0)
                        {
                            return new { Message = 1 };

                        }
                        res.Qty++;
                        db.SaveChanges();


                    }
                    else
                    {
                        var detail = new MarketerFactorItem();
                        detail.Product = product;
                        detail.ProductName = product.Name;
                        detail.Qty = 1;
                        detail.UnitPrice = product.Price - product.Discount;
                        order.MarketerFactorItems.Add(detail);
                        db.SaveChanges();
                    }

                }
                tr.Commit();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                                  .SelectMany(x => x.ValidationErrors)
                                  .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join(" - ", errorMessages);
                return new { Message = 2, Details = fullErrorMessage };
            }


            return new { Message = 0 };

        }

        [MarketerAuthorize]
        [HttpPost]
        [Route("api/MarketerFactor/Index")]
        public object Index()
        {
            var token = HttpContext.Current.Request.Form["Api_Token"];
            int id = db.MarketerUsers.Where(p => p.Api_Token == token).FirstOrDefault().Id;
            var order = db.MarketerFactor.Include("MarketerFactorItems.Product").Where(p => p.MarketerUser.Id == id).Where(p => p.Status == 1)
                .Select(p => new
                { p.Id, p.Buyer, p.BuyerAddress, p.BuyerMobile, p.BuyerPhoneNumber, p.BuyerPostalCode, p.Date, Items = p.MarketerFactorItems.Select(a => new { a.Product.Id, a.Qty, a.UnitPrice, a.ProductName, a.Product.Images, a.Product.Thumbnail }) }
            ).ToList();
            if (order == null)
            {
                return new { Message = 1 };
            }



            return new
            {
                Message = 0,
                Items = order
                // Items = items
            };
        }

        [MarketerAuthorize]
        [HttpPost]
        [Route("api/MarketerFactor/History")]

        public object History()
        {
            var token = HttpContext.Current.Request.Form["Api_Token"];
            int id = db.MarketerUsers.Where(p => p.Api_Token == token).FirstOrDefault().Id;

            var data = db.MarketerFactor.Where(p => p.Status == 0 || p.Status == 2).Where(p => p.MarketerUser.Id == id).OrderByDescending(p => p.Id)
                .Select(p => new { p.Id, p.Date, p.Buyer, p.BuyerAddress, p.BuyerMobile, p.BuyerPhoneNumber, p.BuyerPostalCode, p.Status, p.TotalPrice }).ToList();
            return new
            {
                Message = 0,
                Data = data
            };
        }

        [MarketerAuthorize]
        [HttpPost]
        [Route("api/MarketerFactor/HistoryItems")]

        public object HistoryItems()
        {
            var token = HttpContext.Current.Request.Form["Api_Token"];
            int id = db.MarketerUsers.Where(p => p.Api_Token == token).FirstOrDefault().Id;
            var fid = Convert.ToInt32(HttpContext.Current.Request.Form["Factor_Id"]);

            var data = db.MarketerFactorItem.Include("Product").Where(p => p.MarketerFactor.Id == fid).OrderByDescending(p => p.Id)
                .Select(p => new { p.Id, p.ProductName, p.Qty, p.UnitPrice, p.Product.Main_Image, p.Product.Thumbnail }).ToList();
            return new
            {
                Message = 0,
                Data = data
            };
        }

        [HttpPost]
        [Route("api/MarketerFactor/DeleteProduct")]
        [MarketerAuthorize]

        public object DeleteItem()
        {
            var token = HttpContext.Current.Request.Form["Api_Token"];
            var fid = Convert.ToInt32(HttpContext.Current.Request.Form["Factor_Id"]);
            var item_id = Convert.ToInt32(HttpContext.Current.Request.Form["Item_Id"]);
            int id = db.MarketerUsers.Where(p => p.Api_Token == token).FirstOrDefault().Id;
            var factor = db.MarketerFactor.Where(p => p.Status == 1).Where(p => p.MarketerUser.Id == id).Where(p => p.Id == fid).FirstOrDefault();
            if (factor == null)
            {
                return new { Message = 1 };
            }
            var data = db.MarketerFactorItem.Include("Product.Category").Include("MarketerFactor").Where(p => p.MarketerFactor.Id == factor.Id).Where(p => p.Id == item_id).FirstOrDefault();

            if (data == null)
            {
                return new { Message = 1 };
            }
            //db.Products.Include("Category").Where(p => p.Id == data.Product.Id).FirstOrDefault().Qty += data.Qty;
            db.MarketerFactorItem.Remove(data);
            if (db.MarketerFactorItem.Where(p => p.MarketerFactor.Id == factor.Id).Count()==1)
            {
                db.MarketerFactor.Remove(factor);
            }

            db.SaveChanges();

            return new { Message = 0 };
        }

        [HttpPost]
        [Route("api/MarketerFactor/Finalize")]
        [MarketerAuthorize]

        public object Finalize()
        {
            var tr = db.Database.BeginTransaction();
            var token = HttpContext.Current.Request.Form["Api_Token"];
            var fid = Convert.ToInt32(HttpContext.Current.Request.Form["Factor_Id"]);

            var usr = db.MarketerUsers.Where(p => p.Api_Token == token).FirstOrDefault();
            int id = usr.Id;
            var factor = db.MarketerFactor.Include("MarketerFactorItems.Product.Category").Where(p => p.Id == fid).Where(p => p.Status == 1).Where(p => p.MarketerUser.Id == id).FirstOrDefault();
            if (factor == null)
            {
                return new { Message = 1 };
            }
            List<object> Empty = new List<object>();
            foreach (var item in factor.MarketerFactorItems)
            {
                item.UnitPrice = item.Product.Price - item.Product.Discount;
                item.ProductName = item.Product.Name;
                if (item.Product.Qty < item.Qty)
                    Empty.Add(new { Detail = "محصول " + item.Product.Name + " به تعداد انتخابی شما وجود ندارد" });
            }
            if (Empty.Count > 0)
                return new { Message = 2, Empty };
            factor.TotalPrice = factor.ComputeTotalPrice() + 15000;
            factor.Status = 0;
            factor.Date = DateTime.Now;
            usr.FactorCounter--;
            db.SaveChanges();
            tr.Commit();
            return new { Message = 0 };
        }

        [HttpPost]
        [Route("api/MarketerFactor/ChangeQty")]
        [MarketerAuthorize]

        public object ChangeQty()
        {
            var Qty = Convert.ToInt32(HttpContext.Current.Request.Form["Qty"]);
            var token = HttpContext.Current.Request.Form["Api_Token"];
            var item_id = Convert.ToInt32(HttpContext.Current.Request.Form["Item_Id"]);
            var factor_id = Convert.ToInt32(HttpContext.Current.Request.Form["Factor_Id"]);
            int id = db.MarketerUsers.Where(p => p.Api_Token == token).FirstOrDefault().Id;
            var order = db.MarketerFactor.Where(p => p.MarketerUser.Id == id).Where(p => p.Status == 1).Where(p=>p.Id==factor_id).FirstOrDefault();
            if (order == null)
            {
                return new { Message = 1 };
            }

            var data = db.MarketerFactorItem.Include("Product.Category").Where(p => p.MarketerFactor.Id == order.Id).Where(p => p.Id == item_id).FirstOrDefault();
            if (data == null)
            {
                return new { Message = 1 };
            }

            //data.Product.Qty += data.Qty;
            if (data.Product.Qty < Qty)
            {
                return new { Message = 2 };
            }

            data.Qty = Qty;
            //data.Product.Qty -= Qty;
            db.SaveChanges();
            return new { Message = 0 };

        }


    }
}
