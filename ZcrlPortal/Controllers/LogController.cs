using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZcrlPortal.Models;
using ZcrlPortal.DAL;

namespace ZcrlPortal.Controllers
{
    [Authorize(Roles = "Administrators, Editors, Doctors, TenderGroup")]
    public class LogController : MasterController
    {
        private List<LogRecord> getPagedRecords(int? page, LogRecordType recType, out int maxPageNumber)
        {
            maxPageNumber = 0;
            using (zcrlDbContext = new ZcrlContext())
            {
                List<LogRecord> allRecords;
                allRecords = (from p in zcrlDbContext.LogJournal
                              where (p.RecordType == recType)
                              orderby p.СreatedDate descending
                              select p).ToList();
                maxPageNumber = (int)(Math.Ceiling(allRecords.Count() / 50.0));

                if (!page.HasValue)
                {
                    return allRecords.Take(50).ToList();
                }

                if ((page.Value < 1) || (page.Value > maxPageNumber))
                {
                    return null;
                }
                else
                {
                    return allRecords.Skip(((page.Value - 1) * 10)).Take(50).ToList();
                }
            }
        }

        [Authorize(Roles = "Administrators, TenderGroup")]
        public ActionResult TenderLog(int? page)
        {
            int maxPageNumber = 0;
            List<LogRecord> records = getPagedRecords(page, LogRecordType.TendersAddEdit, out maxPageNumber);
            if(records == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            ViewBag.MaxPageNumber = maxPageNumber;
            ViewBag.ActionName = "TenderLog";
            ViewBag.RecordsToDelete = LogRecordType.TendersAddEdit;

            if (page.HasValue) ViewBag.page = page.Value;
            return View("ViewLog", records);
        }

        [Authorize(Roles = "Administrators, Editors")]
        public ActionResult NewsLog(int? page)
        {
            int maxPageNumber = 0;
            List<LogRecord> records = getPagedRecords(page, LogRecordType.NewsAddEdit, out maxPageNumber);
            if (records == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            ViewBag.MaxPageNumber = maxPageNumber;
            ViewBag.ActionName = "NewsLog";
            ViewBag.RecordsToDelete = LogRecordType.NewsAddEdit;
            if (page.HasValue) ViewBag.page = page.Value;
            return View("ViewLog", records);
        }

        [Authorize(Roles = "Administrators, Editors, Doctors")]
        public ActionResult ArticleLog(int? page)
        {
            int maxPageNumber = 0;
            List<LogRecord> records = getPagedRecords(page, LogRecordType.ArticlesAddEdit, out maxPageNumber);
            if (records == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            ViewBag.MaxPageNumber = maxPageNumber;
            ViewBag.ActionName = "ArticleLog";
            ViewBag.RecordsToDelete = LogRecordType.ArticlesAddEdit;
            if (page.HasValue) ViewBag.page = page.Value;
            return View("ViewLog", records);
        }

        [Authorize(Roles = "Administrators")]
        public ActionResult UsersLog(int? page)
        {
            int maxPageNumber = 0;
            List<LogRecord> records = getPagedRecords(page, LogRecordType.UserChanges, out maxPageNumber);
            if (records == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            ViewBag.MaxPageNumber = maxPageNumber;
            ViewBag.ActionName = "UsersLog";
            ViewBag.RecordsToDelete = LogRecordType.UserChanges;
            if (page.HasValue) ViewBag.page = page.Value;
            return View("ViewLog", records);
        }

        [Authorize(Roles = "Administrators")]
        public ActionResult BannersLog(int? page)
        {
            int maxPageNumber = 0;
            List<LogRecord> records = getPagedRecords(page, LogRecordType.BannerAddEdit, out maxPageNumber);
            if (records == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            ViewBag.MaxPageNumber = maxPageNumber;
            ViewBag.ActionName = "BannersLog";
            ViewBag.RecordsToDelete = LogRecordType.BannerAddEdit;
            if (page.HasValue) ViewBag.page = page.Value;
            return View("ViewLog", records);
        }

        [Authorize(Roles = "Administrators")]
        public ActionResult Delete(LogRecordType recordsGroup)
        {
            string redirectActionName = null;
            using(zcrlDbContext = new ZcrlContext())
            {
                var logsList = (from l in zcrlDbContext.LogJournal where (l.RecordType == recordsGroup) select l);
                
                if(logsList != null)
                {
                    zcrlDbContext.LogJournal.RemoveRange(logsList);
                    zcrlDbContext.SaveChanges();
                }
                switch (recordsGroup)
                {
                    case LogRecordType.UserChanges:
                    case LogRecordType.RegistrationsRequests:
                        {
                            redirectActionName = "UsersLog";
                            break;
                        }
                    case LogRecordType.BannerAddEdit:
                        {
                            redirectActionName = "BannersLog";
                            break;
                        }
                    case LogRecordType.NewsAddEdit:
                        {
                            redirectActionName = "NewsLog";
                            break;
                        }
                    case LogRecordType.ArticlesAddEdit:
                        {
                            redirectActionName = "ArticleLog";
                            break;
                        }
                    case LogRecordType.TendersAddEdit:
                        {
                            redirectActionName = "TenderLog";
                            break;
                        }
                }

                TempData["SuccessMessage"] = "Журнал очіщєно";
                return RedirectToAction(redirectActionName);
            }
        }

    }
}
