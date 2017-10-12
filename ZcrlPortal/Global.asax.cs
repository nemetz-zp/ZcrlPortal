using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using ZcrlPortal.Controllers;
using System.Security.Principal;
using ZcrlPortal.SecurityProviders;

namespace ZcrlPortal
{
    // Примечание: Инструкции по включению классического режима IIS6 или IIS7 
    // см. по ссылке http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.User != null)
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated && !ZcrlMembershipProvider.IsUserExist(HttpContext.Current.User.Identity.Name))
                {
                    FormsAuthentication.SignOut();
                    HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
                }
            }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;
            Response.Clear();
            Server.ClearError();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = "ApplicationError";
            Response.StatusCode = 500;
            if (httpException != null)
            {
                Response.StatusCode = httpException.GetHttpCode();
                switch (Response.StatusCode)
                {
                    case 404:
                        {
                            routeData.Values["action"] = "NotFound";
                            break;
                        }
                }
            }

            IController errorController = new ErrorController();
            var rc = new RequestContext(new HttpContextWrapper(Context), routeData);
            errorController.Execute(rc);
        }
    }
}