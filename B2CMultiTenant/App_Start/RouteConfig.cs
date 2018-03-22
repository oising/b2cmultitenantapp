using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace B2CMultiTenant
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{ttype}/{controller}/{action}/{id}",
                defaults: new { ttype = "b2c", controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
