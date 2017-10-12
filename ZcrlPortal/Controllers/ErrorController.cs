using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ZcrlPortal.Controllers
{
    public class ErrorController : MasterController
    {
        // Ошибки доступа к несуществующим ресурсам
        public ActionResult NotFound()
        {
            return View();
        }

        // Ошибки прав доступа
        public ActionResult AccessError()
        {
            return View();
        }

        // Программные ошибки
        public ActionResult ApplicationError()
        {
            return View();
        }

    }
}
