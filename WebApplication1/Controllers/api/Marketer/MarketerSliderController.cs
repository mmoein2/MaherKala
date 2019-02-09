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
    public class MarketerSliderController : ApiController
    {
        DBContext db = new DBContext();
        [HttpGet]
        [MarketerAuthorize]
        public object GetSliders()
        {
            var list = new { Data = db.MarketerSliders.ToList() };
            return list;
        }
    }
}
