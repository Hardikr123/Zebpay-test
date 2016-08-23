using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace CurrencyExchange.API.Filters
{
    public class CacheClientAttribute : ActionFilterAttribute
    {

        public int Duration { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.Headers.Add("Cache-Control", "public,must-revalidate,max-age=120");
        }
    }
}