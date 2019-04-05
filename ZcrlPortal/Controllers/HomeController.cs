using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ZcrlPortal.DAL;
using ZcrlPortal.ViewModels;
using System.Text.RegularExpressions;
using ZcrlMedicamentModels;
using System.Data.Entity;

namespace ZcrlPortal.Controllers
{
    // Общедуступные ресурсы портала
    [AllowAnonymous]
    public class HomeController : MasterController
    {

        // Новости (page = номер страницы)
        public ActionResult News(int? page)
        {
            ViewBag.Title = "Новини";

            int maxPageNumber = 0;
            using(zcrlDbContext = new ZcrlContext())
            {
                if(page == null)
                {
                    var defaultPortalNews = (from n in zcrlDbContext.PortalPublications.Include("Author")
                                             where (n.InformationType == Models.PublicationType.News)
                                             orderby n.PublicationDate descending
                                             select n).ToList();

                    ViewBag.NewsCount = defaultPortalNews.Count();
                    
                    maxPageNumber = (int)(Math.Ceiling(defaultPortalNews.Count() / 10.0));
                    ViewBag.MaxPageNumber = maxPageNumber;

                    return View("PublicationsList", defaultPortalNews.Take(10).ToList());
                }

                ViewBag.page = page.Value;

                var portalNews = (from n in zcrlDbContext.PortalPublications.Include("Author")
                                  where (n.InformationType == Models.PublicationType.News)
                                  orderby n.PublicationDate descending
                                  select n).ToList();

                maxPageNumber = (int)(Math.Ceiling(portalNews.Count() / 10.0));
                if ((page.Value > maxPageNumber) || (page.Value < 1))
                {
                    return RedirectToAction("NotFound", "Error");
                }

                ViewBag.NewsCount = portalNews.Count();
                ViewBag.MaxPageNumber = maxPageNumber;

                return View("PublicationsList", portalNews.Skip(((page.Value - 1) * 10)).Take(10).ToList());
            }
        }

        // Подробнее о публикации
        public ActionResult PublicationDetails(long? id)
        {
            if((id.HasValue && (id.Value <= 0)) || (!id.HasValue))
            {
                return RedirectToAction("NotFound", "Error");
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                var requiredNews = (from n in zcrlDbContext.PortalPublications.Include("Author").Include("Themes")
                                    where (n.Id == id.Value) 
                                    select n).FirstOrDefault();

                if(requiredNews == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }
                else
                {
                    ViewBag.Title = requiredNews.Title;
                    return View("PublicationDetails", requiredNews);
                }
            }
        }

        // Список статей
        public ActionResult Articles(int? page)
        {
            ViewBag.Title = "Статті";

            if(page.HasValue && page.Value <= 0)
            {
                return RedirectToAction("NotFound", "Error");
            }

            using (zcrlDbContext = new ZcrlContext())
            {

                var portalArticles = (from n in zcrlDbContext.PortalPublications.Include("Author").Include("Themes")
                                      where (n.InformationType == Models.PublicationType.Article) 
                                      orderby n.PublicationDate descending
                                      select n).ToList();

                int maxPageNumber = (int)(Math.Ceiling(portalArticles.Count() / 10.0));
                ViewBag.ArticlesCount = portalArticles.Count();
                ViewBag.MaxPageNumber = maxPageNumber;

                if(!page.HasValue)
                {
                    return View("PublicationsList", portalArticles.Take(10).ToList());
                }

                ViewBag.page = page.Value;
                
                if (page.Value > maxPageNumber)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                return View("PublicationsList", portalArticles.Skip(((page.Value - 1) * 10)).Take(10).ToList());
            }
        }

        // Список сотрудников
        public ActionResult Staff(int? departmentId)
        {
            if(departmentId.HasValue && departmentId.Value <= 0)
            {
                return RedirectToAction("NotFound", "Error");
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                ViewBag.Departments = (from d in zcrlDbContext.PortalDataGroups 
                                       where (d.RelatedGroup == Models.DataGroupType.UserDepartment) 
                                       select d).ToList();

                if (!departmentId.HasValue)
                {
                    var defualtDepartmentStaff = (from p in zcrlDbContext.Profiles
                                           where ((p.DataGroupId == 1) && (p.IsPublicated))
                                           orderby p.ViewPriority ascending
                                           select p).ToList();

                    return View(defualtDepartmentStaff);
                }
                else
                {
                    ViewBag.departmentId = departmentId.Value;
                    var departmentStaff = (from p in zcrlDbContext.Profiles
                                           where (p.DataGroupId == departmentId.Value && p.IsPublicated)
                                           orderby p.ViewPriority ascending
                                           select p).ToList();
                    var requiredDepartment = (from d in zcrlDbContext.PortalDataGroups 
                                              where ((d.Id == departmentId.Value) && (d.RelatedGroup == Models.DataGroupType.UserDepartment))
                                              select d).FirstOrDefault();
                    
                    // Запрос несуществующего подразделения
                    if (requiredDepartment == null)
                    {
                        return RedirectToAction("NotFound", "Error");
                    }

                    return View(departmentStaff);
                }
            }
        }

        public ActionResult UserInfo(int? id)
        {
            if((id.HasValue && (id.Value <= 0)) || (!id.HasValue))
            {
                return RedirectToAction("NotFound", "Error");
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                var requiredProfile = (from p in zcrlDbContext.Profiles 
                                       where (p.Id == id.Value) 
                                       select p).FirstOrDefault();

                if(requiredProfile == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }
                else
                {
                    return View(requiredProfile);
                }
            }
        }

        // Список государственных закупок
        public ActionResult Tender(int? year)
        {
            using(zcrlDbContext = new ZcrlContext())
            {

                if (!year.HasValue)
                {
                    var requiredYear = (from y in zcrlDbContext.TenderItems orderby y.Year.Value descending select y).FirstOrDefault();
                    if(requiredYear != null)
                    {
                        return Tender(requiredYear.Year.Value);
                    }
                    else
                    {
                        return RedirectToAction("News");
                    }
                }

                var yearFiles = (from it in zcrlDbContext.TenderItems.Include("RelatedFile").Include("Year").Include("RelatedGroup").Include("RelatedFile.Author")
                                 where (it.Year.Value == year.Value) select it).ToList();

                var reqiredFiles = (from item in yearFiles
                                    orderby item.PublicationDate descending
                                    group item by item.RelatedGroup into ItemsGroup
                                    select new TenderItemGroup()
                                    {
                                        GroupName = (ItemsGroup.Key == null) ? "Без тендерної категорії" : ItemsGroup.Key.Name,
                                        Items = ItemsGroup.ToList()
                                    }).ToList();
                
                if(reqiredFiles.Count == 0)
                {
                    return RedirectToAction("NotFound", "Error");
                }
                else
                {
                    return View(reqiredFiles);
                }
            }
        }

        // Прочая полезная информация
        public ActionResult Information()
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                var stChap = (from c in zcrlDbContext.Chapters 
                              where (c.ChapterType == Models.StaticChapterType.PortalInformation) 
                              select c).First();
                return View(stChap);
            }
        }

        // История больницы
        public ActionResult History()
        {
            using (zcrlDbContext = new ZcrlContext())
            {
                var stChap = (from c in zcrlDbContext.Chapters
                              where (c.ChapterType == Models.StaticChapterType.History)
                              select c).First();
                return View(stChap);
            }
        }

        public ActionResult Developer()
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                int adminId = (from p in zcrlDbContext.Profiles orderby p.Id ascending select p.Id).First();

                return RedirectToAction("UserInfo", new { id = adminId });
            }
        }

        public ActionResult Download(long? id)
        {
            if(!id.HasValue)
            {
                return RedirectToAction("NotFound", "Error");
            }

            string filePath = null;
            string fileName = null;
            using(zcrlDbContext = new ZcrlContext())
            {
                var fileInDb = (from dbF in zcrlDbContext.UploadFiles where (dbF.Id == id.Value) select dbF).FirstOrDefault();
                if(fileInDb == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }
                else
                {
                    if(string.IsNullOrWhiteSpace(fileInDb.FileName))
                    {
                        return RedirectToAction("NotFound", "Error");
                    }
                    filePath = System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), fileInDb.FileName);
                    fileName = clearFileName(fileInDb.DisplayName) + System.IO.Path.GetExtension(fileInDb.FileName);
                }
                fileInDb.DownloadCount++;
                zcrlDbContext.SaveChanges();
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);

        }

        public ActionResult ShowMedicamentsRemain()
        {
            List<MedGroup> medGroupsList = new List<MedGroup>();

            using(MedicamentsRemainContext mc = new MedicamentsRemainContext())
            {
                List<MedicamentRemain> remainsList = mc.MedicamentRemains
                    .Include(p => p.Medicament)
                    .Include(p => p.Medicament.Meter)
                    .Include(p => p.Medicament.Group)
                    .ToList();

                if (remainsList.Count > 0)
                {
                    ViewBag.LastUpdateDate = remainsList.OrderByDescending(p => p.UpdateDate).First().UpdateDate;

                    medGroupsList = (from item in remainsList
                                     group item by item.Medicament into g1
                                     orderby g1.Key.Name
                                     select new MedicamentRemain
                                     {
                                         Medicament = g1.Key,
                                         CurrentRemain = g1.Sum(p => p.CurrentRemain)
                                     } into gr1
                                     group gr1 by gr1.Medicament.Group into g2
                                     select new MedGroup
                                     {
                                         Name = g2.Key,
                                         MedicamentsRemains = g2.ToList()
                                     } into selection
                                     orderby selection.Name.ViewPriority descending
                                     select selection).ToList();
                }
            }

            return View(medGroupsList);
        }

        private string clearFileName(string fileName)
        {
            Regex illegalInFileName = new Regex(@"[\\/:*?""<>|]");
            return illegalInFileName.Replace(fileName, "-");
        }
    }
}
