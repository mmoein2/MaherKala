using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebApplication1.Filter;
using WebApplication1.Models;
using WebApplication1.Utility;

namespace WebApplication1.Controllers.api
{
    public class UserController : ApiController
    {
        DBContext db = new DBContext();
        [HttpPost]
        [Route("api/User/Register")]
        public object Register()
        {
            string Fullname = HttpContext.Current.Request.Form["Fullname"];
            string Password = HttpContext.Current.Request.Form["Password"];
            string Email = HttpContext.Current.Request.Form["Email"];
            string Address = HttpContext.Current.Request.Form["Address"];
            string PhoneNumber = HttpContext.Current.Request.Form["PhoneNumber"];
            string Mobile = HttpContext.Current.Request.Form["Mobile"];
            string PostalCode = HttpContext.Current.Request.Form["PostalCode"];

            var setting = db.Settings.FirstOrDefault();

            if (db.Users.Any(p => p.Email == Email))
            {
                return new
                {
                    Message = 1,
                };
            }
            if (db.Users.Any(p => p.Mobile == Mobile))
            {
                return new
                {
                    Message = 2,
                };
            }
            Role r = db.Roles.Where(p => p.RoleNameEn == "Member").FirstOrDefault();
            var user = new User();
            user.Role = r;



            user.Status = true;

            if (setting.Email == null || setting.Email == "")
            {
                user.LinkStatus = true;

            }
            else
            {
                user.LinkStatus = false;

                SendEmail s = new Utility.SendEmail(setting);
                string key = Guid.NewGuid().ToString().Replace('-', '0');
                ConfirmEmail c = new ConfirmEmail();
                c.Key = key;
                c.User = user;
                db.ConfirmEmails.Add(c);
                db.SaveChanges();
                var list = new List<string>();
                list.Add(user.Email);
                try
                {
                    s.Send("لینک فعالسازی <br> برروی <a target='_blank' href='" + setting.Domain + "/User/ActiveLink/" + key + "'>این لینک</a> جهت فعالسازی حساب کاربری خود کلیک کنید", "لینک فعالسازی", list);
                }
                catch
                {

                }
            }

            user.Api_Token = Guid.NewGuid().ToString().Replace('-', '0');
            user.Password = DevOne.Security.Cryptography.BCrypt.BCryptHelper.HashPassword(Password, DevOne.Security.Cryptography.BCrypt.BCryptHelper.GenerateSalt());
            user.Email = Email;
            user.Fullname = Fullname;
            user.Address = Address;
            user.PhoneNumber = PhoneNumber;
            db.Users.Add(user);
            db.SaveChanges();
            return new
            {
                Message = 0
            };


        }
        [HttpPost]
        [Route("api/User/Login")]
        public object Login()
        {
            string Email = HttpContext.Current.Request.Form["Email"];
            string Password = HttpContext.Current.Request.Form["Password"];

            var data = db.Users.Where(p => p.Role.Id != 1).Where(p => p.Email == Email).FirstOrDefault();
            if (data == null)
            {
                return new { Message = 1 };
            }
            if (!DevOne.Security.Cryptography.BCrypt.BCryptHelper.CheckPassword(Password, data.Password))
            {
                return new { Message = 2 };
            }
            if (data.Status == false)
            {
                return new { Message = 3 };
            }
            if (data.LinkStatus == false)
            {
                return new { Message = 4 };
            }
            return new
            {
                Message = 0,
                Api_Token = data.Api_Token
            };
        }

        [HttpPost]
        [Route("api/User/UpdateProfile")]
        [ApiAuthorize]

        public object Update()
        {
            var token = HttpContext.Current.Request.Form["Api_Token"];
            var user = db.Users.Where(p => p.Api_Token == token).FirstOrDefault();

            string Fullname = HttpContext.Current.Request.Form["Fullname"];
            string Password = HttpContext.Current.Request.Form["Password"];

            string Address = HttpContext.Current.Request.Form["Address"];
            string PhoneNumber = HttpContext.Current.Request.Form["PhoneNumber"];
            string Mobile = HttpContext.Current.Request.Form["Mobile"];
            string PostalCode = HttpContext.Current.Request.Form["PostalCode"];

            user.Address = Address;
            user.Fullname = Fullname;
            user.Mobile = Mobile;
            if (Password != null)
            {
                user.Password = DevOne.Security.Cryptography.BCrypt.BCryptHelper.HashPassword(Password, DevOne.Security.Cryptography.BCrypt.BCryptHelper.GenerateSalt());
            }
            user.PhoneNumber = PhoneNumber;
            user.PostalCode = PostalCode;
            db.SaveChanges();
            return new { Message = 0 };
        }
        [HttpPost]
        [Route("api/User/ShowProfile")]
        public object ShowProfile()
        {
            var token = HttpContext.Current.Request.Form["Api_Token"];
            var user = db.Users.Where(p => p.Api_Token == token).Select(p => new { p.Address, p.Fullname, p.Email, p.Mobile, p.PhoneNumber, p.PostalCode }).FirstOrDefault();
            return user;
        }
        [HttpPost]
        [Route("api/User/RecoverPassword")]
        [ApiAuthorize]
        public object Recover()
        {
            string key = Guid.NewGuid().ToString().Replace('-', '0');
            var token = HttpContext.Current.Request.Form["Api_Token"];
            var user = db.Users.Where(p => p.Api_Token == token).FirstOrDefault();
            var recover = db.UserRecover.Where(p => p.Status == false && p.User.Id == user.Id).FirstOrDefault();
            if(recover!=null)
            {
                var diff = (DateTime.Now - recover.Time);
                if(diff.Days<=1)
                {
                    return new { Message = 1 };

                }
            }
            UserRecover r = new UserRecover();
            r.Time = DateTime.Now;
            r.User = user;
            r.Key = key;
            r.Status = false;
            db.UserRecover.Add(r);

            Setting setting = db.Settings.FirstOrDefault();
            SendEmail s = new Utility.SendEmail(setting);
            db.SaveChanges();
            var list = new List<string>();
            list.Add(user.Email);
            db.SaveChanges();

            try
            {
                s.Send("<h3>"+setting.SiteName+"<h3>"+"<br/>"+"تغییر رمز عبور<br> برروی <a target='_blank' href='" + setting.Domain + "/User/RecoverUser/" + key + "'>این لینک</a> جهت تغییر رمز عبور کلیک کنید <br/><b> چنانچه شما درخواستی برای تغییر رمز عبور خود صادر نکرده اید، به این ایمیل بی توجه باشید</b>", "لینک فعالسازی", list);
            }
            catch
            {

            }
            return new {Message=0 };
        }
    }
}
