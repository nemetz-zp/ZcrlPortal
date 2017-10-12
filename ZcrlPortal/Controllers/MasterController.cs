using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZcrlPortal.DAL;
using ZcrlPortal.ViewModels;
using System.Net.Mail;

namespace ZcrlPortal.Controllers
{
    public enum CrudMode
    {
        Add, Edit, Delete
    }

    // Реализация логики работы шаблоного представления
    [ValidateInput(false)]
    public class MasterController : Controller
    {

        public const string UPLOADFILE_DIR = "~/UploadFiles";
        public const string UPLOADPHOTO_DIR = "~/UserPhotos";

        protected ZcrlContext zcrlDbContext;

        public MasterController()
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                ViewBag.Banners = (from b in zcrlDbContext.Banners orderby b.ViewPriority ascending select b).ToList();
                ViewBag.RegistrationRequests = (from regReq in zcrlDbContext.UserRegistrationRequests select regReq).ToList().Count();
                ViewBag.TendersList = (from tenItems in zcrlDbContext.TenderItems
                                       group tenItems by tenItems.Year into tenYear
                                       orderby tenYear.Max(t => t.Year.Value) descending
                                       select new ViewTenderYear() { Name = tenYear.Key.Value.ToString(), Value = tenYear.Key.Value }).ToList();
            }
        }

        protected void sendEmail(string receiver, string mailSubject, string message)
        {
            try
            {
                string from = System.Configuration.ConfigurationManager.AppSettings["emailFrom"];
                string pass = System.Configuration.ConfigurationManager.AppSettings["emailPassword"];
                string smtpServer = System.Configuration.ConfigurationManager.AppSettings["smtpServer"];
                int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["smtpPort"]);

                SmtpClient client = new SmtpClient(smtpServer, port);
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(from, pass);

                var notifyMail = new MailMessage(from, receiver);
                notifyMail.Subject = mailSubject;
                notifyMail.Body = message;
                notifyMail.IsBodyHtml = true;

                client.Send(notifyMail);
            }
            catch { }
        }

    }
}
