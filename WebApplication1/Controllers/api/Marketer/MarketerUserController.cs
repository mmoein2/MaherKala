using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebApplication1.Models;

namespace WebApplication1.Controllers.api.Marketer
{
    public class MarketerUserController : ApiController
    {
        DBContext db = new DBContext();
        [HttpPost]
        public object Register()
        {
            string Name = HttpContext.Current.Request.Form["Name"];
            string LastName = HttpContext.Current.Request.Form["LastName"];
            string Mobile = HttpContext.Current.Request.Form["Mobile"];
            string Phone = HttpContext.Current.Request.Form["Phone"];
            string Password = HttpContext.Current.Request.Form["Password"];
            string Address = HttpContext.Current.Request.Form["Address"];

            double Lat = double.Parse(HttpContext.Current.Request.Form["Lat"], CultureInfo.InvariantCulture);

            double Lng = double.Parse(HttpContext.Current.Request.Form["Lng"], CultureInfo.InvariantCulture);

            string IDCardNumber = HttpContext.Current.Request.Form["IDCardNumber"];
            string CertificateNumber = HttpContext.Current.Request.Form["CertificateNumber"];
            bool IsMarrid = Convert.ToBoolean(HttpContext.Current.Request.Form["IsMarrid"]);
            string AccountNumber = HttpContext.Current.Request.Form["AccountNumber"];
            string CardAccountNumber = (HttpContext.Current.Request.Form["CardAccountNumber"]);
            string IBNA = HttpContext.Current.Request.Form["IBNA"];
            string Description = HttpContext.Current.Request.Form["Description"];
            string Parent_Id = HttpContext.Current.Request.Form["Parent_Id"];


            if (Password.Length<8)
            {
                return new { StatusCode = 1, Message = "طول رمز عبور حداقل باید هشت رقم باشد" };
            }
            

            MarketerUser m = new MarketerUser();
            m.Name = Name;
            m.LastName = LastName;
            m.Mobile = Mobile;
            m.Phone = Phone;
            m.Password = DevOne.Security.Cryptography.BCrypt.BCryptHelper.HashPassword(Password, DevOne.Security.Cryptography.BCrypt.BCryptHelper.GenerateSalt());
            m.Address = Address;
            m.Lat = Lat;
            m.Lng = Lng;
            m.IDCardNumber = IDCardNumber;
            m.CertificateNumber = CertificateNumber;
            m.IsMarrid = IsMarrid;
            m.AccountNumber = AccountNumber;
            m.CardAccountNumber = CardAccountNumber;
            m.IBNA = IBNA;
            m.Description = Description;
            m.IsAvailable = false;
            m.Parent_Id = 0;
            if (!HttpContext.Current.Request.Form.AllKeys.Contains("IDCardPicture"))
            {
                return new { StatusCode=1,Message="تصویر کارت ملی را ارسال کنید"};
            }
            var data = HttpContext.Current.Request["IDCardPicture"];
            data = data.Replace("data:image/png;base64,", "");
            data = data.Replace("data:image/jpeg;base64,", "");
            data = data.Replace(" ", "+");
            byte[] imgBytes = Convert.FromBase64String(data);

           

            if (imgBytes.Length < 10000)
            {
                return new { StatusCode = 1, Message = "تصویر کارت ملی با مشکل مواجه است" };
            }
            if(imgBytes.Length >= 5242880)
            {
                return new { StatusCode = 1, Message = "تصویر کارت ملی با حجم بیش از پنج مگابایت مجاز نیست" };
            }
            if (db.MarketerUsers.Any(p => p.IDCardNumber == IDCardNumber))
            {
                return new { StatusCode = 1, Message = "شماره ملی تکراری است" };
            }

            if (db.MarketerUsers.Any(p => p.Mobile == Mobile))
            {
                return new { StatusCode = 1, Message = "شماره موبایل تکراری است" };
            }
            //var img = HttpContext.Current.Request.Files[0];

           
            var unique = Guid.NewGuid().ToString().Replace('-', '0') + "." + "jpg";
            var imageUrl = "/Upload/MarketerUpload/" + unique;
            string path = HttpContext.Current.Server.MapPath(imageUrl);
            m.IDCardPhotoAddress = imageUrl;


            

            m.Api_Token = Guid.NewGuid().ToString().Replace('-', '0');
            if(Parent_Id!=null)
            {
                if(db.MarketerUsers.Any(p=>p.Id==Convert.ToInt32(Parent_Id)))
                {
                    m.Parent_Id = Convert.ToInt32(Parent_Id);
                }
            }
            m.IsFirstTime = true;
            db.MarketerUsers.Add(m);
            try
            {

                db.SaveChanges();
                using (var imageFile = new FileStream(path, FileMode.Create))
                {
                    imageFile.Write(imgBytes, 0, imgBytes.Length);
                    imageFile.Flush();
                }
                return new { StatusCode=0};
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                  .SelectMany(x => x.ValidationErrors)
                  .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join(" - ", errorMessages);
                return new { StatusCode = 1, Message = fullErrorMessage };
            }
            
            
            
        }

        [HttpPost]
        [Route("api/MarketerUser/Login")]
        public object Login()
        {
            string Mobile = HttpContext.Current.Request.Form["Mobile"];
            string Password = HttpContext.Current.Request.Form["Password"];
            var user = db.MarketerUsers.Where(p => p.Mobile == Mobile).FirstOrDefault();
            if (user==null)
            {
                return new { StatusCode = 1, Message = "شماره موبایل یا رمز عبور صحیح نیست" };
            }
            if (user.IsAvailable==false)
            {
                return new { StatusCode = 2, Message = "نام کاربری شما هنوز فعال نشده است" };
            }
            if (!DevOne.Security.Cryptography.BCrypt.BCryptHelper.CheckPassword(Password,user.Password))
            {
                return new { StatusCode = 1, Message = "شماره موبایل یا رمز عبور صحیح نیست" };
            }
            return new { StatusCode = 0, Api_Token= user.Api_Token };
        }
    }
}
