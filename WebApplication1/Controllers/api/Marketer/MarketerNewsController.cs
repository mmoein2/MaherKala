using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication1.Filter;
using WebApplication1.Models;

namespace WebApplication1.Controllers.api.Marketer
{
    public class MarketerNewsController : ApiController
    {
        DBContext db = new DBContext();
        [HttpGet]
        [Route("api/MarketerNews/GetNews")]
        [MarketerAuthorize]
        public object GetNews()
        {
            var data = db.MarketerNews.OrderByDescending(p=>p.Id);
            var paged = new PagedItem<MarketerNews>(data,"/api/MarketerNews/GetNews");
            return new { Data = paged, Message = 0};

        }
        [HttpGet]
        [MarketerAuthorize]
        [Route("api/MarketerNews/GetPrizes")]
        public object GetPrizes()
        {
            var data = db.MarketerPrizes.OrderByDescending(p => p.Id);
            var paged = new PagedItem<MarketerPrize>(data, "/api/MarketerNews/GetPrizes");
            return new { Data = paged, Message = 0 };

        }
    }
}
