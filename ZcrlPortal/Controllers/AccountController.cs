using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Net.Mail;
using ZcrlPortal.ViewModels;
using ZcrlPortal.Models;
using ZcrlPortal.DAL;

namespace ZcrlPortal.Controllers
{
    [AllowAnonymous]
    public class AccountController : MasterController
    {
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if(User.Identity.IsAuthenticated)
            {
                return RedirectToAction("AccessError", "Error");
            }

            FormsAuthentication.SignOut();

            ViewBag.redirectUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel lm, string redirectUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("News", "Home");
            }

            using (zcrlDbContext = new ZcrlContext())
            {
                var existRequest = (from r in zcrlDbContext.UserRegistrationRequests where (r.Login == lm.Login) select r).FirstOrDefault();
                if (existRequest != null)
                {
                    ViewBag.LoginError = "Обліковий запис з цим логін ще не активований. Повторіть свою спробу пізніше або зв'яжітся із адміністрацією веб-порталу.";
                    ViewBag.redirectUrl = redirectUrl;
                    return View(lm);
                }
            }

            FormsAuthentication.SignOut();

            if (Membership.ValidateUser(lm.Login, lm.Password))
            {
                FormsAuthentication.SetAuthCookie(lm.Login, true);

                if (Url.IsLocalUrl(redirectUrl))
                {
                    return Redirect(redirectUrl);
                }
                else
                {
                    return RedirectToAction("News", "Home");
                }
            }
            else
            {
                ViewBag.LoginError = "Не вірний логін або пароль";
                ViewBag.redirectUrl = redirectUrl;
                return View(lm);
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("News", "Home");
            }
            RegistrationRequest newRR = new RegistrationRequest() { Sex = UserSex.Female };
            return View(newRR);
        }

        [HttpPost]
        public ActionResult Register(RegistrationRequest request)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("News", "Home");
            }

            if(ModelState.IsValid)
            {
                using (zcrlDbContext = new ZcrlContext())
                {
                    var existUser = (from u in zcrlDbContext.Users where (u.Login == request.Login) select u).FirstOrDefault();
                    var existRequest = (from r in zcrlDbContext.UserRegistrationRequests where (r.Login == request.Login) select r).FirstOrDefault();

                    if (existUser != null || existRequest != null)
                    {
                        ViewBag.RegistrationError = "Такий логін вже зареєстрований";
                        return View(request);
                    }

                    if (!string.IsNullOrWhiteSpace(request.Email))
                    {
                        var existUserEmail = (from p in zcrlDbContext.Profiles where (p.Email == request.Email.ToLower()) select p).FirstOrDefault();
                        var existRequestEmail = (from r in zcrlDbContext.UserRegistrationRequests where (r.Email == request.Email.ToLower()) select r).FirstOrDefault();

                        if (existUserEmail != null || existRequestEmail != null)
                        {
                            ViewBag.RegistrationError = "Така адреса електронної пошти вже зареєстрована";
                            return View(request);
                        }
                    }
                }

                ViewBag.RegistrationSuccess = true;
                using(zcrlDbContext = new ZcrlContext())
                {
                    zcrlDbContext.UserRegistrationRequests.Add(request);
                    zcrlDbContext.SaveChanges();
                }

                // Уведомляем администратора о регистрации
                sendEmail("web-swat@yandex.ru", 
                    "ЗАЯВКА НА РЕЄСТРАЦІЮ",
                    string.Format("<b>{0} {1} {2} подав(ла) заявку на реєстрацію на порталі Запорізької ЦРЛ</b>", 
                    request.LastName, request.FirstName, request.MiddleName));

                return View();
            }
            else
            {
                ViewBag.RegistrationError = ModelState.Values.First(f => f.Errors.Count() >= 1).Errors.First().ErrorMessage;
                return View(request);
            }
        }


        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("News", "Home");
        }

    }
}
